namespace Domain.Primitives;

public interface IDomainEvent
{
    Guid EventId { get; init; }
    string ToJson();
}
