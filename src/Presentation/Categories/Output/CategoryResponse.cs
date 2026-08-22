using Domain.Entities;

namespace Presentation.Categories.Output;

public record CategoryResponse(
    int Id,
    string Name,
    string Description,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsHighlighted,
    int HighlightOrder,
    string? HighlightColor,
    string? HighlightLink
)
{
    public static CategoryResponse FromEntity(Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.CreatedBy,
        category.CreatedAt,
        category.UpdatedAt,
        category.IsHighlighted,
        category.HighlightOrder,
        category.HighlightColor,
        category.HighlightLink
    );
}
