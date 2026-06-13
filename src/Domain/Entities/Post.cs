using Domain.ValueObjects;

namespace Domain.Entities;

public class Post : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public Image CoverImage { get; private set; } = Image.Empty;
    public DateTimeOffset? PublishedAt { get; private set; }
    public int AuthorId { get; private set; }
    public Author? Author { get; private set; }

    protected Post() { }

    public Post(string title, string slug, string summary, string content, int authorId)
    {
        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        AuthorId = authorId;
    }

    public void Update(string title, string slug, string summary, string content)
    {
        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCoverImage(Image image)
    {
        CoverImage = image;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unpublish()
    {
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsPublished => PublishedAt.HasValue;
}
