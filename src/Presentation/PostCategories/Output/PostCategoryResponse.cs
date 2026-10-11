using Domain.Entities;

namespace Presentation.PostCategories.Output;

public record PostCategoryResponse(
    int Id,
    string Name,
    string Slug,
    string? Color,
    bool IsActive,
    bool IsHighlighted,
    int HighlightOrder,
    string? HighlightIcon,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static PostCategoryResponse FromEntity(PostCategory category) => new(
        category.Id,
        category.Name,
        category.Slug,
        category.Color,
        category.IsActive,
        category.IsHighlighted,
        category.HighlightOrder,
        category.HighlightIcon,
        category.CreatedAt,
        category.UpdatedAt
    );
}
