namespace Presentation.PostCategories.Input;

public record UpdatePostCategoryRequest(
    string Name,
    string? Color = null
);
