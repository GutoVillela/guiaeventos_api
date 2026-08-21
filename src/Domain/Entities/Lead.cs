using Domain.Primitives;

namespace Domain.Entities;

public class Lead : Entity
{
    protected Lead() { }

    public Lead(string name, string email, string phone, string? company, int advertisementId, string advertisementType)
    {
        Source = "Advertisement";
        Name = name;
        Email = email.ToLowerInvariant();
        Phone = phone;
        Company = company;
        AdvertisementId = advertisementId;
        AdvertisementType = advertisementType;
    }

    public Lead(string name, string email, string? subject, string? message)
    {
        Source = "ContactForm";
        Name = name;
        Email = email.ToLowerInvariant();
        Subject = subject;
        Message = message;
    }

    public string Source { get; private set; } = "Advertisement";
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Company { get; private set; }
    public int? AdvertisementId { get; private set; }
    public string? AdvertisementType { get; private set; }
    public string? Subject { get; private set; }
    public string? Message { get; private set; }
    public bool IsRead { get; private set; } = false;
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
