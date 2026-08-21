using Domain.Entities;

namespace Presentation.Services.Output;

public record CategorySummary(int Id, string Name);
public record ImageResponse(string Url, string? AltText);

public record ServiceResponse(
    int Id,
    string Slug,
    string Name,
    string Description,
    string Summary,
    string Status,
    bool IsHighlighted,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? RejectionReason,
    string? RejectedBy,
    DateTimeOffset? RejectedAt,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string? Phone,
    string? VideoUrl,
    IEnumerable<CategorySummary> Categories,
    IEnumerable<ImageResponse> Images
)
{
    public static ServiceResponse FromEntity(Service service) => new(
        service.Id,
        service.Slug,
        service.Name,
        service.Description,
        service.Summary,
        service.Status.ToString(),
        service.IsHighlighted,
        service.CreatedBy,
        service.CreatedAt,
        service.UpdatedAt,
        service.RejectionReason,
        service.RejectedBy,
        service.RejectedAt,
        service.ApprovedBy,
        service.ApprovedAt,
        service.Phone?.ToString(),
        service.VideoUrl,
        service.Categories.Select(c => new CategorySummary(c.Id, c.Name)),
        service.Images.Select(i => new ImageResponse(i.Url, i.AltText))
    );
}
