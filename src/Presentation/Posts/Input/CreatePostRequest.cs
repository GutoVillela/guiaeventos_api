namespace Presentation.Posts.Input;

public record CreatePostRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    int AuthorId,
    string? CoverImageUrl = null,
    string? CoverImageAltText = null
);
