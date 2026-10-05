using Domain.Entities;

namespace Presentation.Posts.Output;

public record PostCategorySummary(
    int Id,
    string Name,
    string Slug,
    string? Color
)
{
    public static PostCategorySummary FromEntity(PostCategory category) => new(
        category.Id,
        category.Name,
        category.Slug,
        category.Color
    );
}
