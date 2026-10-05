using Domain.Primitives;

namespace Domain.Entities;

public class PostCategory : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IList<Post> Posts { get; private set; } = new List<Post>();

    protected PostCategory() { }

    public PostCategory(string name, string slug, string? color)
    {
        Name = name;
        Slug = slug;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
    }

    public void Update(string name, string slug, string? color)
    {
        Name = name;
        Slug = slug;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
