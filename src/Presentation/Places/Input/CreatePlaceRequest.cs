namespace Presentation.Places.Input;

public record CreatePlaceRequest(
    string Name,
    string Description,
    string? Summary,
    int AdvertiserId,
    IEnumerable<int> CategoryIds,
    string Street,
    string? Neighborhood,
    string City,
    string State,
    string Country,
    string? ZipCode,
    string? Number,
    string? Complement,
    string? ReferencePoint
);
