using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Tests.ValueObjects;

public class FixedTaxItemTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        var item = new FixedTaxItem("Community Tax", 100m);

        Assert.Equal("Community Tax", item.Name);
        Assert.Equal(100m, item.Amount);
    }

    [Fact]
    public void Constructor_ZeroAmount_SetsProperties()
    {
        var item = new FixedTaxItem("Tax", 0m);

        Assert.Equal(0m, item.Amount);
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FixedTaxItem("Tax", -1m));
    }
}

public class FlatRateTaxItemTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        var item = new FlatRateTaxItem("Pension Tax", 0.2m);

        Assert.Equal("Pension Tax", item.Name);
        Assert.Equal(0.2m, item.Rate);
    }

    [Fact]
    public void Constructor_ZeroRate_SetsProperties()
    {
        var item = new FlatRateTaxItem("Tax", 0m);

        Assert.Equal(0m, item.Rate);
    }

    [Fact]
    public void Constructor_RateOfOne_SetsProperties()
    {
        var item = new FlatRateTaxItem("Tax", 1m);

        Assert.Equal(1m, item.Rate);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_InvalidRate_ThrowsArgumentOutOfRangeException(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlatRateTaxItem("Tax", rate));
    }
}

public class ProgressiveTaxItemTests
{
    [Fact]
    public void Constructor_ValidIntervals_SortsByThreshold()
    {
        var intervals = new List<ProgressiveInterval>
        {
            new(null, 0.4m),
            new(1000m, 0.1m),
            new(5000m, 0.2m)
        };

        var item = new ProgressiveTaxItem("Income Tax", intervals);

        Assert.Equal("Income Tax", item.Name);
        Assert.Equal(3, item.Intervals.Count);
        Assert.Equal(1000m, item.Intervals[0].Threshold);
        Assert.Equal(5000m, item.Intervals[1].Threshold);
        Assert.Null(item.Intervals[2].Threshold);
    }

    [Fact]
    public void Constructor_SingleOpenEndedInterval_Succeeds()
    {
        var intervals = new List<ProgressiveInterval>
        {
            new(null, 0.3m)
        };

        var item = new ProgressiveTaxItem("Tax", intervals);

        Assert.Single(item.Intervals);
        Assert.Null(item.Intervals[0].Threshold);
    }

    [Fact]
    public void Constructor_NullIntervals_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProgressiveTaxItem("Tax", null!));
    }

    [Fact]
    public void Constructor_EmptyIntervals_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProgressiveTaxItem("Tax", []));
    }

    [Fact]
    public void Constructor_NoOpenEndedInterval_ThrowsArgumentException()
    {
        var intervals = new List<ProgressiveInterval>
        {
            new(1000m, 0.1m),
            new(5000m, 0.2m)
        };

        Assert.Throws<ArgumentException>(() => new ProgressiveTaxItem("Tax", intervals));
    }

    [Fact]
    public void Constructor_MultipleOpenEndedIntervals_ThrowsArgumentException()
    {
        var intervals = new List<ProgressiveInterval>
        {
            new(null, 0.1m),
            new(1000m, 0.2m),
            new(null, 0.3m)
        };

        Assert.Throws<ArgumentException>(() => new ProgressiveTaxItem("Tax", intervals));
    }
}
