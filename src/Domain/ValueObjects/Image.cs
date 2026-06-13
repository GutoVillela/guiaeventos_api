namespace Domain.ValueObjects;

public record Image
{
    public string Url { get; init; }
    public string? AltText { get; init; }
    // EF Constructor
    private Image() { }
    public Image(string url, string? altText)
    {
        Url = url;
        AltText = altText;
    }
    public static Image Create(string url, string? altText)
    {
        return new Image(url, altText);
    }

    public static Image Empty => new Image(string.Empty, null);
}
