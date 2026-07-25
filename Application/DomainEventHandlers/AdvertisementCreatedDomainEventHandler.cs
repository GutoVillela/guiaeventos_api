using Domain.DomainEvents;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
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
        int? referenceId = null;
        ENotificationReferenceType? referenceType = null;

        if (int.TryParse(domainEvent.ReferenceId, out var advertisementId))
        {
            referenceId = advertisementId;
            var isPlace = await _dbContext.Places.AnyAsync(p => p.Id == advertisementId, cancellationToken);
            referenceType = isPlace ? ENotificationReferenceType.Place : ENotificationReferenceType.Service;
        }
        else
        {
            _logger.LogWarning("AdvertisementCreatedDomainEvent has no valid ReferenceId; notification will have no reference link.");
        }

        var notification = new Notification(
            "Um novo anúncio foi criado",
            "Um novo anúncio foi criado e precisa de aprovação. Clique aqui para visualizar.",
            referenceId,
            referenceType);

        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
