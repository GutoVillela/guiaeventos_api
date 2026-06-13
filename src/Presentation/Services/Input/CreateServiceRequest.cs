namespace Presentation.Services.Input;

public record CreateServiceRequest(
    string Name,
    string Description,
    string? Summary,
    int AdvertiserId,
    IEnumerable<int> CategoryIds
);
