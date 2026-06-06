namespace Domain.ValueObjects;

public record Phone
{
    public string AreaCode { get; init; }
    public string Number { get; init; }

    public Phone(string areaCode, string number)
    {
        if (string.IsNullOrWhiteSpace(areaCode))
            throw new ArgumentException("Area code cannot be empty", nameof(areaCode));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));

        AreaCode = areaCode;
        Number = number;
    }
}
