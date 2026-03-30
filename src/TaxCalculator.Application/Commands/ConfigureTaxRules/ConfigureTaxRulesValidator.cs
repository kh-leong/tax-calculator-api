using FluentValidation;

namespace TaxCalculator.Application.Commands.ConfigureTaxRules;

public class ConfigureTaxRulesValidator : AbstractValidator<ConfigureTaxRulesCommand>
{
    public ConfigureTaxRulesValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Matches("^[a-zA-Z]{2,3}$")
            .WithMessage("Country code must be 2 or 3 letters.");

        RuleFor(x => x.TaxItems)
            .NotEmpty()
            .WithMessage("At least one tax item is required.");

        RuleFor(x => x.TaxItems)
            .Must(items => items?.OfType<ProgressiveTaxItemDto>().Count() <= 1)
            .When(x => x.TaxItems is not null)
            .WithMessage("At most one progressive tax item is allowed.");

        RuleForEach(x => x.TaxItems).SetInheritanceValidator(v =>
        {
            v.Add(new FixedTaxItemValidator());
            v.Add(new FlatRateTaxItemValidator());
            v.Add(new ProgressiveTaxItemValidator());
        });
    }
}

internal class FixedTaxItemValidator : AbstractValidator<FixedTaxItemDto>
{
    public FixedTaxItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}

internal class FlatRateTaxItemValidator : AbstractValidator<FlatRateTaxItemDto>
{
    public FlatRateTaxItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Rate).InclusiveBetween(0, 1);
    }
}

internal class ProgressiveTaxItemValidator : AbstractValidator<ProgressiveTaxItemDto>
{
    public ProgressiveTaxItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Intervals).NotEmpty().WithMessage("At least one interval is required.");

        RuleForEach(x => x.Intervals).ChildRules(interval =>
        {
            interval.RuleFor(b => b.Rate).InclusiveBetween(0, 1);
            interval.RuleFor(b => b.Threshold).GreaterThanOrEqualTo(0).When(b => b.Threshold.HasValue);
        });

        RuleFor(x => x.Intervals)
            .Must(intervals => intervals?.Count(i => i.Threshold is null) == 1)
            .When(x => x.Intervals is { Count: > 0 })
            .WithMessage("Exactly one interval must have a null threshold (open-ended).");

        RuleFor(x => x.Intervals)
            .Must(intervals =>
            {
                var sorted = intervals!
                    .OrderBy(i => i.Threshold ?? decimal.MaxValue)
                    .ToList();
                return sorted[^1].Threshold is null;
            })
            .When(x => x.Intervals is { Count: > 0 } && x.Intervals.Count(i => i.Threshold is null) == 1)
            .WithMessage("The open-ended interval (null threshold) must be the highest threshold.");
    }
}
