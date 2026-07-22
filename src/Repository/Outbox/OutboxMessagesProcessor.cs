using Domain.DomainEvents;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repository.Persistence;
using Shared.Outbox;

namespace Repository.Outbox;

public sealed class OutboxMessagesProcessor : IOutboxProcessor
{

    private readonly AppDbContext _dataContext;
    private readonly ILogger<OutboxMessagesProcessor> _logger;
    private readonly IDomainEventHandler<AdvertisementCreatedDomainEvent> _domainEventHandler;

    public OutboxMessagesProcessor(
        AppDbContext dataContext, 
        ILogger<OutboxMessagesProcessor> logger,
        IDomainEventHandler<AdvertisementCreatedDomainEvent> domainEventHandler
        )
    {
        _dataContext = dataContext;
        _logger = logger;
        _domainEventHandler = domainEventHandler;
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default, int batchSize = 1000, TimeSpan? delay = null)
    {
        var messages = await _dataContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogInformation("No outbox messages to process");
            return;
        }

        foreach(var message in messages)
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                message.Payload,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            if(domainEvent is null)
            {
                _logger.LogError("Failed to deserialize domain event from outbox message with ID {MessageId}", message.Id);
                // TODO Handle outbox message error
                continue;
            }

            try
            {
                await ProcessDomainEvent(domainEvent, cancellationToken);
                message.MaskAsProcessed();
                _logger.LogInformation("Processed outbox message with ID: {MessageId}", message.Id);
            }
            catch (Exception ex) 
            {
                // TODO Handle exception
                message.FailOrRetry(ex.Message);
            }
        }

        if (delay.HasValue)
            await Task.Delay(delay.Value, cancellationToken);

        _dataContext.OutboxMessages.UpdateRange(messages);
        await _dataContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Processed {Count} outbox messages", messages.Count);
    }

    private async Task ProcessDomainEvent(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // TODO Melhorar isso aqui depois, porque fazer isso um por um pra cada evento é sacanagem.....
        if (domainEvent.GetType().Name == nameof(AdvertisementCreatedDomainEvent))
            await _domainEventHandler.HandleAsync(domainEvent as AdvertisementCreatedDomainEvent, cancellationToken);
    }
}
