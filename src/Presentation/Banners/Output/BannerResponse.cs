using Domain.Entities;

namespace Presentation.Banners.Output;

public record BannerResponse(
    int Id,
    string Title,
    string? Description,
    string LinkUrl,
    string ImageUrl,
    string? ImageAltText,
    int Order,
    bool IsActive,
    bool IsCurrentlyVisible,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static BannerResponse FromEntity(Banner banner) => new(
        banner.Id,
        banner.Title,
        banner.Description,
        banner.LinkUrl,
        banner.Image.Url,
        banner.Image.AltText,
        banner.Order,
        banner.IsActive,
        banner.IsCurrentlyVisible,
        banner.StartsAt,
        banner.EndsAt,
        banner.CreatedBy,
        banner.CreatedAt,
        banner.UpdatedAt
    );
}
