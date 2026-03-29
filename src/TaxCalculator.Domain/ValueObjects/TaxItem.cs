namespace TaxCalculator.Domain.ValueObjects;

public abstract record TaxItem(string Name);

public record FixedTaxItem : TaxItem
{
    public decimal Amount { get; }

    public FixedTaxItem(string name, decimal amount) : base(name)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

        Amount = amount;
    }
}

public record FlatRateTaxItem : TaxItem
{
    public decimal Rate { get; }

    public FlatRateTaxItem(string name, decimal rate) : base(name)
    {
        if (rate < 0 || rate > 1)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be between 0 and 1.");

        Rate = rate;
    }
}

public record ProgressiveTaxItem : TaxItem
{
    public IReadOnlyList<ProgressiveInterval> Intervals { get; }

    public ProgressiveTaxItem(string name, IReadOnlyList<ProgressiveInterval> intervals) : base(name)
    {
        if (intervals is null || intervals.Count == 0)
            throw new ArgumentException("At least one interval is required.", nameof(intervals));

        // TODO: use a sort that explicitly puts null at the end.
        // current implementation has a collision on decimal.MaxValue, but it's unlikely to be an issue in practice.
        var sorted = intervals
            .OrderBy(b => b.Threshold ?? decimal.MaxValue)
            .ToList();

        if (sorted[^1].Threshold is not null)
            throw new ArgumentException("The last interval must have a null threshold (open-ended).", nameof(intervals));

        if (sorted[..^1].Any(i => i.Threshold is null))
            throw new ArgumentException("Exactly one interval may have a null threshold (open-ended), and it must be the last after sorting.", nameof(intervals));
        Intervals = sorted.AsReadOnly();
    }
}
