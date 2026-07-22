using Domain.DomainEvents;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.Extensions.Logging;
using Repository.Persistence;

namespace Application.DomainEventHandlers;

public class AdvertisementCreatedDomainEventHandler : IDomainEventHandler<AdvertisementCreatedDomainEvent>
{

    private readonly ILogger<AdvertisementCreatedDomainEventHandler> _logger;
    private readonly AppDbContext _dbContext;

    public AdvertisementCreatedDomainEventHandler(ILogger<AdvertisementCreatedDomainEventHandler> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(AdvertisementCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Notification notification = new("Um novo anúncio foi criado", "Um novo anúncio foi criado e precisa de aprovação. Clique aqui para visualizar");
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
