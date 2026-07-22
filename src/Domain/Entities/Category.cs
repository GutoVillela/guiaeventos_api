using Domain.Primitives;

namespace Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
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
}
