namespace Presentation.Categories.Input;

public record CreateCategoryRequest(
    string Name,
    string? Description
);
