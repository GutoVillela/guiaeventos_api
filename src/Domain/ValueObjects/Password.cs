namespace Domain.ValueObjects;

public record Password
{
    public string Hash { get; set; }
}
