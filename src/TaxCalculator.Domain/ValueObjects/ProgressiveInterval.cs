namespace TaxCalculator.Domain.ValueObjects;

public record ProgressiveInterval
{
    public decimal? Threshold { get; }
    public decimal Rate { get; }

    public ProgressiveInterval(decimal? threshold, decimal rate)
    {
        if (rate < 0 || rate > 1)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be between 0 and 1.");

        if (threshold is < 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be non-negative.");

        Threshold = threshold;
        Rate = rate;
    }
}
