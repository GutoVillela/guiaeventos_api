namespace Domain.DomainEvents;

public sealed record AdvertisementCreatedDomainEvent : DomainEventBase
{
    public AdvertisementCreatedDomainEvent(Guid eventId) : base(eventId) { }
}
