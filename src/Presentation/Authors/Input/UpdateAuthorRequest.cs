namespace Presentation.Authors.Input;

public record UpdateAuthorRequest(
    string Name,
    string Email,
    string? Bio = null
);
