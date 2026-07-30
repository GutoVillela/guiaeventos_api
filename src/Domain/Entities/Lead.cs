using Domain.Primitives;

namespace Domain.Entities;

public class Lead : Entity
{
    protected Lead() { }

    public Lead(string name, string email, string phone, string? company, int advertisementId, string advertisementType)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Company = company;
        AdvertisementId = advertisementId;
        AdvertisementType = advertisementType;
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Company { get; private set; }
    public int AdvertisementId { get; private set; }
    public string AdvertisementType { get; private set; } = string.Empty;
    public bool IsRead { get; private set; } = false;
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
