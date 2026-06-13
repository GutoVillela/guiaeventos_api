using Domain.Entities;

namespace Presentation.Authors.Output;

public record AuthorResponse(
    int Id,
    string Name,
    string Email,
    string? Bio,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static AuthorResponse FromEntity(Author author) => new(
        author.Id,
        author.Name,
        author.Email,
        author.Bio,
        author.CreatedBy,
        author.CreatedAt,
        author.UpdatedAt
    );
}
