using FluentValidation;

namespace TaxCalculator.Application.Queries.CalculateTax;

public class CalculateTaxValidator : AbstractValidator<CalculateTaxRequest>
{
    public CalculateTaxValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Matches("^[a-zA-Z]{2,3}$")
            .WithMessage("Country code must be 2 or 3 letters.");

        RuleFor(x => x.GrossSalary)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Gross salary must be non-negative.");
    }
}
