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
        RuleFor(x => x.Brackets).NotEmpty().WithMessage("At least one bracket is required.");

        RuleForEach(x => x.Brackets).ChildRules(bracket =>
        {
            bracket.RuleFor(b => b.Rate).InclusiveBetween(0, 1);
            bracket.RuleFor(b => b.Threshold).GreaterThanOrEqualTo(0).When(b => b.Threshold.HasValue);
        });
    }
}
