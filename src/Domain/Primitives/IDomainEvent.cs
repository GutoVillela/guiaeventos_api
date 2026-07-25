namespace Domain.Primitives;

public interface IDomainEvent
{
    string? ReferenceId { get; set; }
    string ToJson();
}
