using Domain.ValueObjects;

namespace Domain.Entities;

public class Banner : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string LinkUrl { get; private set; } = string.Empty;
    public Image Image { get; private set; } = Image.Empty;
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? StartsAt { get; private set; }
    public DateTimeOffset? EndsAt { get; private set; }

    protected Banner() { }

    public Banner(string title, string linkUrl, Image image, int order = 0,
                  string? description = null, DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
    {
        Title = title;
        LinkUrl = linkUrl;
        Image = image;
        Order = order;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = true;
    }

    public void Update(string title, string linkUrl, Image image, int order,
                       string? description = null, DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
    {
        Title = title;
        LinkUrl = linkUrl;
        Image = image;
        Order = order;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
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

    public bool IsCurrentlyVisible =>
        IsActive &&
        (StartsAt == null || StartsAt <= DateTimeOffset.UtcNow) &&
        (EndsAt == null || EndsAt > DateTimeOffset.UtcNow);
}
