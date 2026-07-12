namespace Presentation.Places.Input;

public record UpdatePlaceRequest(
    string Name,
    string Description,
    string? Summary,
    string Street,
    string? Neighborhood,
    string City,
    string State,
    string Country,
    string? ZipCode,
    string? Number,
    string? Complement,
    string? ReferencePoint,
    int[] CategoryIds,
    string? PhoneAreaCode,
    string? PhoneNumber
);
