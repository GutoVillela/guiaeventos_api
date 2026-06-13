using System.Security.Cryptography;
using System.Text;

namespace Domain.ValueObjects;

public record Password
{
    public string Hash { get; private set; } = string.Empty;

    internal Password() { }

    private Password(string hash)
    {
        Hash = hash;
    }

    public static Password Create(string rawPassword)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword));
        return new Password(Convert.ToHexString(bytes).ToLowerInvariant());
    }

    public bool Verify(string rawPassword)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword));
        var inputHash = Convert.ToHexString(bytes).ToLowerInvariant();
        return string.Equals(Hash, inputHash, StringComparison.Ordinal);
    }
}
