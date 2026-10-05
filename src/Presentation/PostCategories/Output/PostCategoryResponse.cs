using Domain.Entities;

namespace Presentation.PostCategories.Output;

public record PostCategoryResponse(
    int Id,
    string Name,
    string Slug,
    string? Color,
    bool IsActive,
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
        category.CreatedAt,
        category.UpdatedAt
    );
}
