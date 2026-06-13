namespace Presentation.Categories.Input;

public record UpdateCategoryRequest(
    string Name,
    string? Description
);
