using Domain.Entities;
using Domain.ValueObjects;

namespace Presentation.Places.Output;

public record CategorySummary(int Id, string Name);

public record ImageResponse(string Url, string? AltText);

public record PlaceResponse(
    int Id,
    string Name,
    string Description,
    string Summary,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? RejectionReason,
    string? RejectedBy,
    DateTimeOffset? RejectedAt,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string? Phone,
    LocationResponse Location,
    IEnumerable<CategorySummary> Categories,
    IEnumerable<ImageResponse> Images
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
        place.RejectionReason,
        place.RejectedBy,
        place.RejectedAt,
        place.ApprovedBy,
        place.ApprovedAt,
        place.Phone?.ToString(),
        LocationResponse.FromAddress(place.Location),
        place.Categories.Select(c => new CategorySummary(c.Id, c.Name)),
        place.Images.Select(i => new ImageResponse(i.Url, i.AltText))
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
