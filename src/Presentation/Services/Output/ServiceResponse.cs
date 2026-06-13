using Domain.Entities;

namespace Presentation.Services.Output;

public record CategorySummary(int Id, string Name);

public record ServiceResponse(
    int Id,
    string Name,
    string Description,
    string Summary,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IEnumerable<CategorySummary> Categories
)
{
    public static ServiceResponse FromEntity(Service service) => new(
        service.Id,
        service.Name,
        service.Description,
        service.Summary,
        service.Status.ToString(),
        service.CreatedBy,
        service.CreatedAt,
        service.UpdatedAt,
        service.Categories.Select(c => new CategorySummary(c.Id, c.Name))
    );
}
