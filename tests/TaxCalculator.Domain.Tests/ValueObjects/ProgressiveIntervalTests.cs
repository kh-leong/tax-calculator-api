using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Tests.ValueObjects;

public class ProgressiveIntervalTests
{
    [Fact]
    public void Constructor_ValidThresholdAndRate_SetsProperties()
    {
        var interval = new ProgressiveInterval(1000m, 0.1m);

        Assert.Equal(1000m, interval.Threshold);
        Assert.Equal(0.1m, interval.Rate);
    }

    [Fact]
    public void Constructor_NullThreshold_SetsProperties()
    {
        var interval = new ProgressiveInterval(null, 0.5m);

        Assert.Null(interval.Threshold);
        Assert.Equal(0.5m, interval.Rate);
    }

    [Fact]
    public void Constructor_ZeroThresholdAndRate_SetsProperties()
    {
        var interval = new ProgressiveInterval(0m, 0m);

        Assert.Equal(0m, interval.Threshold);
        Assert.Equal(0m, interval.Rate);
    }

    [Fact]
    public void Constructor_RateOfOne_SetsProperties()
    {
        var interval = new ProgressiveInterval(500m, 1m);

        Assert.Equal(1m, interval.Rate);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(-1)]
    [InlineData(2)]
    public void Constructor_InvalidRate_ThrowsArgumentOutOfRangeException(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveInterval(1000m, rate));
    }

    [Fact]
    public void Constructor_NegativeThreshold_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveInterval(-1m, 0.1m));
    }
}
