namespace Presentation.Services.Input;

public record UpdateServiceRequest(
    string Name,
    string Description,
    string? Summary,
    int[] CategoryIds,
    string? PhoneAreaCode,
    string? PhoneNumber
);
