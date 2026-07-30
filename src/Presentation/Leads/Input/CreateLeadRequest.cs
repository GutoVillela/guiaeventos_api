namespace Presentation.Leads.Input;

public record CreateLeadRequest(
    string Name,
    string Email,
    string Phone,
    string? Company,
    int AdvertisementId,
    string AdvertisementType
);
