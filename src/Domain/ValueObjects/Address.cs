using System.ComponentModel.DataAnnotations;

namespace Domain.ValueObjects;

public record Address
{
    [Required] public string Street { get; init; }
    public string? Neighborhood { get; init; }
    [Required] public string City { get; init; }
    [Required] public string State { get; init; }
    [Required] public string Country { get; init; }
    public string? ZipCode { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public string? ReferencePoint { get; init; }

    // EF Constructor
    private Address() { }

    public Address(
        string street,
        string neighborhood,
        string city,
        string state,
        string country,
        string zipCode,
        string number,
        string complement,
        string referencePoint)
    {
        Street = street;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
        Number = number;
        Complement = complement;
        ReferencePoint = referencePoint;
    }

    public static Address Create(
        string street,
        string neighborhood,
        string city,
        string state,
        string country,
        string zipCode,
        string number,
        string complement,
        string referencePoint)
    {
        return new Address(street, neighborhood, city, state, country, zipCode, number, complement, referencePoint);
    }

    public static Address Empty => new Address("", "", "", "", "", "", "", "", "");
}
