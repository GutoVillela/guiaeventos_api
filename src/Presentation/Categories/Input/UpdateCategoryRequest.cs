namespace Presentation.Categories.Input;

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    bool IsHighlighted = false,
    int HighlightOrder = 0,
    string? HighlightColor = null,
    string? HighlightLink = null
);
