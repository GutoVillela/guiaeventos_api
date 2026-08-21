namespace Presentation.Leads.Input;

public record CreateLeadRequest(
    string Source,
    string Name,
    string Email,
    string? Phone,
    string? Company,
    int? AdvertisementId,
    string? AdvertisementType,
    string? Subject,
    string? Message
);
