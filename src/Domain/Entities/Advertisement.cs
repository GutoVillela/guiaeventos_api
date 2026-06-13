using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public abstract class Advertisement : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public EAdvertisementStatus Status { get; private set; } = EAdvertisementStatus.PendingApproval;
    public string? Website { get; private set; }
    public Phone? Phone { get; private set; }

    public User Advertiser { get; private set; } = new User();
    public IList<Category> Categories { get; private set; } = new List<Category>();
    public IList<Image> Images { get; private set; } = new List<Image>();

    protected Advertisement() { }

    protected Advertisement(string name, string description, string summary, User advertiser)
    {
        Name = name;
        Description = description;
        Summary = summary;
        Advertiser = advertiser;
    }

    protected void UpdateBase(string name, string description, string summary)
    {
        Name = name;
        Description = description;
        Summary = summary;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetWebsite(string? website)
    {
        Website = website;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPhone(Phone? phone)
    {
        Phone = phone;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddImage(Image image)
    {
        Images.Add(image);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveImage(Image image)
    {
        Images.Remove(image);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetImages(IEnumerable<Image> images)
    {
        Images.Clear();
        foreach (var image in images)
            Images.Add(image);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCategories(IEnumerable<Category> categories)
    {
        Categories.Clear();
        foreach (var category in categories)
            Categories.Add(category);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
