using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Entities;

public class CountryTaxConfiguration
{
    public CountryCode Code { get; }
    public IReadOnlyList<TaxItem> Items { get; }

    public CountryTaxConfiguration(CountryCode code, IReadOnlyList<TaxItem> items)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (items is null || items.Count == 0)
            throw new ArgumentException("At least one tax item is required.", nameof(items));

        if (items.OfType<ProgressiveTaxItem>().Count() > 1)
            throw new ArgumentException("At most one progressive tax item is allowed.", nameof(items));

        Code = code;
        Items = new List<TaxItem>(items).AsReadOnly();
    }
}
