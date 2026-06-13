namespace Presentation.Authors.Input;

public record CreateAuthorRequest(
    string Name,
    string Email,
    string? Bio = null
);
