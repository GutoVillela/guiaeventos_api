namespace Presentation.PostCategories.Input;

public record CreatePostCategoryRequest(
    string Name,
    string? Color = null,
    bool IsHighlighted = false,
    int HighlightOrder = 0,
    string? HighlightIcon = null
);
