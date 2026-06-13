using Domain.Entities;

namespace Presentation.Posts.Output;

public record PostResponse(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    int AuthorId,
    string? AuthorName,
    string? CoverImageUrl,
    string? CoverImageAltText,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static PostResponse FromEntity(Post post) => new(
        post.Id,
        post.Title,
        post.Slug,
        post.Summary,
        post.Content,
        post.AuthorId,
        post.Author?.Name,
        post.CoverImage.Url == string.Empty ? null : post.CoverImage.Url,
        post.CoverImage.AltText,
        post.IsPublished,
        post.PublishedAt,
        post.CreatedBy,
        post.CreatedAt,
        post.UpdatedAt
    );
}
