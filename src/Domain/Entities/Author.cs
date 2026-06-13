namespace Domain.Entities;

public class Author : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Bio { get; private set; }

    public IEnumerable<Post> Posts { get; private set; } = new List<Post>();

    protected Author() { }

    public Author(string name, string email, string? bio = null)
    {
        Name = name;
        Email = email;
        Bio = bio;
    }

    public void Update(string name, string email, string? bio = null)
    {
        Name = name;
        Email = email;
        Bio = bio;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
