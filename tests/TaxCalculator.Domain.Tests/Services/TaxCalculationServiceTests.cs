using FluentAssertions;
using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Tests.Services;

public class TaxCalculationServiceTests
{
    [Fact]
    public void Calculate_FullSpecExample_ReturnsExpectedResult()
    {
        // the exact example given
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new FixedTaxItem("CommunityTax", 1500),
                new FixedTaxItem("RadioTax", 500),
                new FlatRateTaxItem("PensionTax", 0.20m),
                new ProgressiveTaxItem("IncomeTax",
                [
                    new ProgressiveInterval(10000, 0.00m),
                    new ProgressiveInterval(30000, 0.20m),
                    new ProgressiveInterval(null, 0.40m)
                ])
            ]);

        var result = TaxCalculationService.Calculate(config, 62000);

        result.GrossSalary.Should().Be(62000);
        result.TaxableBase.Should().Be(60000);
        result.TotalTaxes.Should().Be(30000);
        result.NetSalary.Should().Be(32000);

        result.Breakdown.Should().HaveCount(4);
        result.Breakdown.Should().ContainSingle(b => b.Name == "CommunityTax" && b.Amount == 1500);
        result.Breakdown.Should().ContainSingle(b => b.Name == "RadioTax" && b.Amount == 500);
        result.Breakdown.Should().ContainSingle(b => b.Name == "PensionTax" && b.Amount == 12000);
        result.Breakdown.Should().ContainSingle(b => b.Name == "IncomeTax" && b.Amount == 16000);
    }

    [Fact]
    public void Calculate_FixedTaxOnly_ReducesTaxableBase()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [new FixedTaxItem("Tax", 2000)]);

        var result = TaxCalculationService.Calculate(config, 10000);

        result.TaxableBase.Should().Be(8000);
        result.TotalTaxes.Should().Be(2000);
        result.NetSalary.Should().Be(8000);
    }

    [Fact]
    public void Calculate_FixedExceedsGross_TaxableBaseIsZero()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [new FixedTaxItem("Tax", 5000)]);

        var result = TaxCalculationService.Calculate(config, 1000);

        result.TaxableBase.Should().Be(0);
        result.TotalTaxes.Should().Be(5000);
        result.NetSalary.Should().Be(-4000);
    }

    [Fact]
    public void Calculate_FlatRateOnly_AppliesPercentageToGross()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [new FlatRateTaxItem("Tax", 0.10m)]);

        var result = TaxCalculationService.Calculate(config, 10000);

        result.TaxableBase.Should().Be(10000);
        result.TotalTaxes.Should().Be(1000);
        result.NetSalary.Should().Be(9000);
    }

    [Fact]
    public void Calculate_MultipleFlatRates_AllApplied()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new FlatRateTaxItem("Tax1", 0.10m),
                new FlatRateTaxItem("Tax2", 0.05m)
            ]);

        var result = TaxCalculationService.Calculate(config, 10000);

        result.TotalTaxes.Should().Be(1500);
        result.Breakdown.Should().HaveCount(2);
    }

    [Fact]
    public void Calculate_ProgressiveOnly_AppliesBracketsIncrementally()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new ProgressiveTaxItem("Income",
                [
                    new ProgressiveInterval(10000, 0.00m),
                    new ProgressiveInterval(30000, 0.20m),
                    new ProgressiveInterval(null, 0.40m)
                ])
            ]);

        var result = TaxCalculationService.Calculate(config, 60000);

        // 10k@0% = 0, 20k@20% = 4000, 30k@40% = 12000
        result.TotalTaxes.Should().Be(16000);
        result.NetSalary.Should().Be(44000);
    }

    [Fact]
    public void Calculate_ProgressiveSingleOpenBracket_AppliesRateToAll()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new ProgressiveTaxItem("FlatProgressive",
                [
                    new ProgressiveInterval(null, 0.25m)
                ])
            ]);

        var result = TaxCalculationService.Calculate(config, 40000);

        result.TotalTaxes.Should().Be(10000);
    }

    [Fact]
    public void Calculate_ZeroGross_AllZeros()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [new FlatRateTaxItem("Tax", 0.20m)]);

        var result = TaxCalculationService.Calculate(config, 0);

        result.GrossSalary.Should().Be(0);
        result.TaxableBase.Should().Be(0);
        result.TotalTaxes.Should().Be(0);
        result.NetSalary.Should().Be(0);
    }

    [Fact]
    public void Calculate_ProgressiveIntervalsUnordered_SortedInternally()
    {
        // intervals provided out of order should still calculate correctly
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new ProgressiveTaxItem("Income",
                [
                    new ProgressiveInterval(null, 0.40m),
                    new ProgressiveInterval(10000, 0.10m),
                    new ProgressiveInterval(30000, 0.20m)
                ])
            ]);

        var result = TaxCalculationService.Calculate(config, 50000);

        // 10k@10% = 1000, 20k@20% = 4000, 20k@40% = 8000
        result.TotalTaxes.Should().Be(13000);
    }

    [Fact]
    public void Calculate_NegativeGrossSalary_ThrowsArgumentOutOfRangeException()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [new FlatRateTaxItem("Tax", 0.20m)]);

        var act = () => TaxCalculationService.Calculate(config, -1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("grossSalary");
    }

    [Fact]
    public void Calculate_NullConfiguration_ThrowsArgumentNullException()
    {
        var act = () => TaxCalculationService.Calculate(null!, 50000);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }
}
