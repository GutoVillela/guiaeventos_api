using System.Reflection;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Repository.Outbox;

namespace Repository.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Entities being Added have Id = 0 — their domain events' ReferenceId must be fixed after INSERT.
        // Collect them now (before state changes) so we can update them after the first save.
        var addedWithEvents = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.State == EntityState.Added && e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // For existing entities (Modified/Deleted), Id is already set — update ReferenceId before the
        // interceptor serialises their events during the first save.
        foreach (var entry in ChangeTracker.Entries<Entity>().Where(e => e.State != EntityState.Added))
            entry.Entity.UpdateDomainEventsReferenceId();

        bool ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            // First save: EF Core executes INSERTs/UPDATEs.
            // Interceptor handles non-Added entities; Added entities are intentionally skipped.
            var result = await base.SaveChangesAsync(cancellationToken);

            if (addedWithEvents.Count > 0)
            {
                // Auto-increment Ids are now assigned — fix ReferenceId on their domain events.
                foreach (var entity in addedWithEvents)
                    entity.UpdateDomainEventsReferenceId();

                // Second save: interceptor processes the newly-inserted entities' events.
                await base.SaveChangesAsync(cancellationToken);
            }

            if (ownsTransaction && transaction is not null)
                await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            if (ownsTransaction && transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public DbSet<Place> Places => Set<Place>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Lead> Leads => Set<Lead>();
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global soft delete filters — Advertisement covers Place and Service via TPH inheritance
        modelBuilder.Entity<Advertisement>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Author>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Banner>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Post>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Lead>().HasQueryFilter(x => !x.IsDeleted);
    }
}
