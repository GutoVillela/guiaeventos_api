namespace Presentation.PostCategories.Input;

public record CreatePostCategoryRequest(
    string Name,
    string? Color = null
);
