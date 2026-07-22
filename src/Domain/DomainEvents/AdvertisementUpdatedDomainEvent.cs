namespace Domain.DomainEvents;

public record AdvertisementUpdatedDomainEvent : DomainEventBase
{
    public AdvertisementUpdatedDomainEvent(Guid advertisementClientId) : base(advertisementClientId.ToString()) { }
}