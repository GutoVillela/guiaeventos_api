using Domain.Primitives;

namespace Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsHighlighted { get; set; }
    public int HighlightOrder { get; set; }
    public string? HighlightColor { get; set; }
    public string? HighlightLink { get; set; }
    public IEnumerable<Advertisement> Advertisements { get; private set; } = new List<Advertisement>();

    protected Category() { }

    public Category(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetHighlight(bool isHighlighted, int order, string? color, string? link)
    {
        IsHighlighted = isHighlighted;
        HighlightOrder = order;
        HighlightColor = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        HighlightLink = string.IsNullOrWhiteSpace(link) ? null : link.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
