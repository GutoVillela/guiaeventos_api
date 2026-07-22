namespace Domain.DomainEvents;

public sealed record AdvertisementCreatedDomainEvent : DomainEventBase
{
    public AdvertisementCreatedDomainEvent(Guid advertisementClientId) : base(advertisementClientId.ToString()) { }
}
