using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Web;
using Repository.Outbox;
using Domain.Primitives;

namespace Repository.Interceptors;

internal class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if(dbContext is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var outboxMessages = dbContext.ChangeTracker.Entries<Entity>()
            .Where(e => e.State != EntityState.Added) // Added entities have Id = 0; AppDbContext fixes ReferenceId after INSERT
            .SelectMany(e =>
            {
                var entity = e.Entity;
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent =>
            {
                return new OutboxMessage(
                    id: Guid.NewGuid(),
                    type: domainEvent.GetType()?.FullName,
                    payload: JsonConvert.SerializeObject(
                        domainEvent,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        })
                    );
            }).ToList();
        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
