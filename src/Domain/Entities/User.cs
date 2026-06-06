using Domain.ValueObjects;

namespace Domain.Entities;

public class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty; 
    public Password Password { get; private set; } = new Password();

    public IEnumerable<Advertisement> Advertisements { get; private set; } = new List<Advertisement>();
}
