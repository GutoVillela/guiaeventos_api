namespace Presentation.Posts.Input;

public record UpdatePostRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    string? CoverImageUrl = null,
    string? CoverImageAltText = null
);
