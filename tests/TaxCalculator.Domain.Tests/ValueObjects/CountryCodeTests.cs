using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Tests.ValueObjects;

public class CountryCodeTests
{
    [Theory]
    [InlineData("DE", "DE")]
    [InlineData("de", "DE")]
    [InlineData("US", "US")]
    [InlineData("us", "US")]
    [InlineData("uK", "UK")]
    [InlineData("USA", "USA")]
    [InlineData(" us ", "US")]
    public void Create_ValidCode_ReturnsUppercaseTrimmedCode(string input, string expected)
    {
        var code = CountryCode.Create(input);
        Assert.Equal(expected, code.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsArgumentException(string? input)
    {
        Assert.Throws<ArgumentException>(() => CountryCode.Create(input!));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABCD")]
    public void Create_InvalidLength_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => CountryCode.Create(input));
    }

    [Theory]
    [InlineData("U1")]
    [InlineData("1A")]
    [InlineData("A-B")]
    public void Create_NonLetterCharacters_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => CountryCode.Create(input));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var code = CountryCode.Create("DE");
        Assert.Equal("DE", code.ToString());
    }

    [Fact]
    public void Equality_SameCode_AreEqual()
    {
        var code1 = CountryCode.Create("DE");
        var code2 = CountryCode.Create("de");
        Assert.Equal(code1, code2);
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        var code1 = CountryCode.Create("DE");
        var code2 = CountryCode.Create("UK");
        Assert.NotEqual(code1, code2);
    }
}
