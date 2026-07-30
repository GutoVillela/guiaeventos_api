using Domain.Entities;

namespace Presentation.Leads.Output;

public record LeadResponse(
    int Id,
    string Name,
    string Email,
    string Phone,
    string? Company,
    int AdvertisementId,
    string AdvertisementType,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt
)
{
    public static LeadResponse FromEntity(Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Email,
        lead.Phone,
        lead.Company,
        lead.AdvertisementId,
        lead.AdvertisementType,
        lead.IsRead,
        lead.ReadAt,
        lead.CreatedAt
    );
}
