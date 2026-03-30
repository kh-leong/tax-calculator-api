using FluentAssertions;
using TaxCalculator.Application.Commands.ConfigureTaxRules;

namespace TaxCalculator.Application.Tests.Commands;

public class ConfigureTaxRulesValidatorTests
{
    private readonly ConfigureTaxRulesValidator _validator = new();

    [Fact]
    public void ProgressiveIntervals_Valid_Passes()
    {
        var command = new ConfigureTaxRulesCommand("US",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(10000, 0.10m),
                new ProgressiveIntervalDto(50000, 0.20m),
                new ProgressiveIntervalDto(null, 0.30m)
            ])
        ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProgressiveIntervals_SingleOpenEnded_Passes()
    {
        var command = new ConfigureTaxRulesCommand("US",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(null, 0.15m)
            ])
        ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProgressiveIntervals_NoOpenEnded_Fails()
    {
        var command = new ConfigureTaxRulesCommand("US",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(10000, 0.10m),
                new ProgressiveIntervalDto(50000, 0.20m)
            ])
        ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("null threshold"));
    }

    [Fact]
    public void ProgressiveIntervals_MultipleOpenEnded_Fails()
    {
        var command = new ConfigureTaxRulesCommand("US",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(10000, 0.10m),
                new ProgressiveIntervalDto(null, 0.20m),
                new ProgressiveIntervalDto(null, 0.30m)
            ])
        ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("null threshold"));
    }

    [Fact]
    public void TaxItems_Null_FailsWithOnlyRequiredError()
    {
        var command = new ConfigureTaxRulesCommand("DE", null!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TaxItems")
            .Which.ErrorMessage.Should().Be("At least one tax item is required.");
    }

    [Fact]
    public void TaxItems_Empty_FailsWithOnlyRequiredError()
    {
        var command = new ConfigureTaxRulesCommand("DE", []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TaxItems")
            .Which.ErrorMessage.Should().Be("At least one tax item is required.");
    }

    [Fact]
    public void TaxItems_MultipleProgressiveItems_Fails()
    {
        var command = new ConfigureTaxRulesCommand("DE",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(null, 0.10m)
            ]),
            new ProgressiveTaxItemDto("Tax",
            [
                new ProgressiveIntervalDto(null, 0.05m)
            ])
        ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "At most one progressive tax item is allowed.");
    }
}
