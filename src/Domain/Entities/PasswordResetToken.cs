namespace Domain.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;

    internal PasswordResetToken() { }

    public PasswordResetToken(int userId, string token, int expirationMinutes = 30)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        IsUsed = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsValid() => !IsUsed && DateTimeOffset.UtcNow <= ExpiresAt;

    public void MarkAsUsed() => IsUsed = true;
}
