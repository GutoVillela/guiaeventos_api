using Domain.Entities;
using Domain.ValueObjects;

namespace Presentation.Places.Output;

public record CategorySummary(int Id, string Name);

public record PlaceResponse(
    int Id,
    string Name,
    string Description,
    string Summary,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    LocationResponse Location,
    IEnumerable<CategorySummary> Categories
)
{
    public static PlaceResponse FromEntity(Place place) => new(
        place.Id,
        place.Name,
        place.Description,
        place.Summary,
        place.Status.ToString(),
        place.CreatedBy,
        place.CreatedAt,
        place.UpdatedAt,
        LocationResponse.FromAddress(place.Location),
        place.Categories.Select(c => new CategorySummary(c.Id, c.Name))
    );
}

public record LocationResponse(
    string Street,
    string? Neighborhood,
    string City,
    string State,
    string Country,
    string? ZipCode,
    string? Number,
    string? Complement,
    string? ReferencePoint
)
{
    public static LocationResponse FromAddress(Address address) => new(
        address.Street,
        address.Neighborhood,
        address.City,
        address.State,
        address.Country,
        address.ZipCode,
        address.Number,
        address.Complement,
        address.ReferencePoint
    );
}
