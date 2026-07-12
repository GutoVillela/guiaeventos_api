using Domain.Converters;
using Domain.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.DomainEvents;

public record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; }

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new PolymorphicTypeResolver(),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
        return JsonSerializer.Serialize(this, GetType(), options);
    }

    public static T FromJson<T>(string json) where T : IDomainEvent
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new PolymorphicTypeResolver(),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        return JsonSerializer.Deserialize<T>(json, options);
    }

    protected DomainEventBase() { }

    public DomainEventBase(Guid eventId)
    {
        this.EventId = eventId;
    }
}

