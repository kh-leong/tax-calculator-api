using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Tests.Entities;

public class CountryTaxConfigurationTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        var code = CountryCode.Create("DE");
        var items = new List<TaxItem> { new FixedTaxItem("Community Tax", 100m) };

        var config = new CountryTaxConfiguration(code, items);

        Assert.Equal(code, config.Code);
        Assert.Single(config.Items);
    }

    [Fact]
    public void Constructor_MultipleItemTypes_Succeeds()
    {
        var code = CountryCode.Create("DE");
        var items = new List<TaxItem>
        {
            new FixedTaxItem("Fixed", 50m),
            new FlatRateTaxItem("Flat", 0.1m),
            new ProgressiveTaxItem("Progressive", [new ProgressiveInterval(null, 0.2m)])
        };

        var config = new CountryTaxConfiguration(code, items);

        Assert.Equal(3, config.Items.Count);
    }

    [Fact]
    public void Constructor_NullCode_ThrowsArgumentNullException()
    {
        var items = new List<TaxItem> { new FixedTaxItem("Tax", 100m) };

        Assert.Throws<ArgumentNullException>(() => new CountryTaxConfiguration(null!, items));
    }

    [Fact]
    public void Constructor_NullItems_ThrowsArgumentException()
    {
        var code = CountryCode.Create("DE");

        Assert.Throws<ArgumentException>(() => new CountryTaxConfiguration(code, null!));
    }

    [Fact]
    public void Constructor_EmptyItems_ThrowsArgumentException()
    {
        var code = CountryCode.Create("DE");

        Assert.Throws<ArgumentException>(() => new CountryTaxConfiguration(code, []));
    }

    [Fact]
    public void Constructor_MultipleProgressiveItems_ThrowsArgumentException()
    {
        var code = CountryCode.Create("DE");
        var items = new List<TaxItem>
        {
            new ProgressiveTaxItem("Progressive1", [new ProgressiveInterval(null, 0.2m)]),
            new ProgressiveTaxItem("Progressive2", [new ProgressiveInterval(null, 0.3m)])
        };

        Assert.Throws<ArgumentException>(() => new CountryTaxConfiguration(code, items));
    }

    [Fact]
    public void Constructor_SingleProgressiveItem_Succeeds()
    {
        var code = CountryCode.Create("DE");
        var items = new List<TaxItem>
        {
            new FixedTaxItem("Fixed", 50m),
            new ProgressiveTaxItem("Progressive", [new ProgressiveInterval(null, 0.2m)])
        };

        var config = new CountryTaxConfiguration(code, items);

        Assert.Equal(2, config.Items.Count);
    }
}
