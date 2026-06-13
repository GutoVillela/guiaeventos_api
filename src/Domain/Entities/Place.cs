using Domain.ValueObjects;

namespace Domain.Entities;

public class Place : Advertisement
{
    public Address Location { get; private set; } = Address.Empty;

    protected Place() { }

    public Place(string name, string description, string summary, User advertiser, Address location)
        : base(name, description, summary, advertiser)
    {
        Location = location;
    }

    public void Update(string name, string description, string summary, Address location)
    {
        UpdateBase(name, description, summary);
        Location = location;
    }
}
