using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Outbox;

namespace Repository.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Place> Places => Set<Place>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Post> Posts => Set<Post>();
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
    }
}
