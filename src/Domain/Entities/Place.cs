using Domain.ValueObjects;

namespace Domain.Entities;

public class Place : Advertisement
{
    public Address Location { get; private set; } = Address.Empty;
}
