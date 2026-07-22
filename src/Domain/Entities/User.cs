using Domain.Enums;
using Domain.Primitives;
using Domain.ValueObjects;

namespace Domain.Entities;

public class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public EUserRole Role { get; private set; } = EUserRole.Advertiser;
    public Password Password { get; private set; } = new Password();
    public Phone? Phone { get; private set; }

    public IEnumerable<Advertisement> Advertisements { get; private set; } = new List<Advertisement>();

    internal User() { }

    public User(string name, string username, string email, string rawPassword)
    {
        Name = name;
        Username = username;
        Email = email;
        Password = Domain.ValueObjects.Password.Create(rawPassword);
        IsActive = true;
    }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetRole(EUserRole role)
    {
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPhone(Phone? phone)
    {
        Phone = phone;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePassword(string rawPassword)
    {
        Password = Domain.ValueObjects.Password.Create(rawPassword);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
