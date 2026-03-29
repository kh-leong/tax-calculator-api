namespace TaxCalculator.Domain.ValueObjects;

public record class CountryCode
{
    public string Value { get; }

    private CountryCode(string value) => Value = value;

    public static CountryCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code cannot be empty.", nameof(code));

        var trimmed = code.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 3)
            throw new ArgumentException("Country code must be 2 or 3 characters.", nameof(code));

        if (!trimmed.All(char.IsLetter))
            throw new ArgumentException("Country code must contain only letters.", nameof(code));

        return new CountryCode(trimmed.ToUpperInvariant());
    }

    public override string ToString() => Value;
}
