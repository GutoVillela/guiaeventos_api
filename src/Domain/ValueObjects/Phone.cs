namespace Domain.ValueObjects;

public record Phone
{
    public string AreaCode { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;

    internal Phone() { }

    public Phone(string areaCode, string number)
    {
        if (string.IsNullOrWhiteSpace(areaCode))
            throw new ArgumentException("Area code cannot be empty.", nameof(areaCode));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty.", nameof(number));

        AreaCode = areaCode;
        Number = number;
    }

    public static Phone Create(string areaCode, string number) => new(areaCode, number);

    public override string ToString() => $"({AreaCode}) {Number}";
}
