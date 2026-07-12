namespace Domain.DomainEvents;

public record AdvertisementUpdatedDomainEvent : DomainEventBase
{
    public AdvertisementUpdatedDomainEvent(Guid eventId) : base(eventId) { }
}