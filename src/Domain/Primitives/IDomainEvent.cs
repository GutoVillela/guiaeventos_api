namespace Domain.Primitives;

public interface IDomainEvent
{
    string? ReferenceId { get; init; }
    string ToJson();
}
