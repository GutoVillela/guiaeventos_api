using System.ComponentModel.DataAnnotations;

namespace Domain.Primitives;

public abstract class Entity
{
    private readonly IList<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];

    [Key]
    public int Id { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null) throw new ArgumentNullException(nameof(domainEvent), "Domain event cannot be null");
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() { _domainEvents.Clear(); }

    // Called by AppDbContext after INSERT so that auto-increment Id is already assigned.
    public void UpdateDomainEventsReferenceId()
    {
        foreach (var domainEvent in _domainEvents)
            domainEvent.ReferenceId = Id.ToString();
    }
}
