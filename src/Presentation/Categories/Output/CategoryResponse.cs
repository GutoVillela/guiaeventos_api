using Domain.Entities;

namespace Presentation.Categories.Output;

public record CategoryResponse(
    int Id,
    string Name,
    string Description,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static CategoryResponse FromEntity(Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.CreatedBy,
        category.CreatedAt,
        category.UpdatedAt
    );
}
