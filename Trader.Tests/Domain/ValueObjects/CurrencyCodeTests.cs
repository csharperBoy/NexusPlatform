using FluentAssertions;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Domain.ValueObjects;

public class CurrencyCodeTests
{
    [Theory]
    [InlineData("IRR", "IRR")]
    [InlineData("usd", "USD")]
    [InlineData("  USD  ", "USD")]
    [InlineData("EUR", "EUR")]
    [InlineData("AED", "AED")]
    [InlineData("gold", "GOLD")]
    [InlineData("SILVER", "SILVER")]
    [InlineData("copper", "COPPER")]
    [InlineData("Platinum", "PLATINUM")]
    [InlineData("OIL", "OIL")]
    public void RequireValid_WithSupportedCurrency_ReturnsNormalized(
        string input, string expected)
    {
        CurrencyCode.RequireValid(input, "test").Should().Be(expected);
    }

    [Theory]
    [InlineData("USDD")]      // typo
    [InlineData("XYZ")]       // unknown
    [InlineData("A")]         // too short
    [InlineData("ABCDEFGHIJK")] // too long
    public void RequireValid_WithInvalidCurrency_Throws(string input)
    {
        var act = () => CurrencyCode.RequireValid(input, "test");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("test");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireValid_WithNullOrEmpty_Throws(string? input)
    {
        var act = () => CurrencyCode.RequireValid(input, "test");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("test");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeOrNull_WithNullOrEmpty_ReturnsNull(string? input)
    {
        CurrencyCode.NormalizeOrNull(input, "test").Should().BeNull();
    }

    [Fact]
    public void NormalizeOrNull_WithValidCurrency_ReturnsNormalized()
    {
        CurrencyCode.NormalizeOrNull("usd", "test").Should().Be("USD");
    }

    [Fact]
    public void NormalizeOrNull_WithInvalidCurrency_Throws()
    {
        var act = () => CurrencyCode.NormalizeOrNull("USDD", "test");

        act.Should().Throw<ArgumentException>();
    }
    [Fact]
    public void All_ContainsBothFiatAndCommodities()
    {
        var all = CurrencyCode.All;

        all.Should().Contain(new[] { "IRR", "USD", "EUR", "AED" });
        all.Should().Contain(new[] { "GOLD", "SILVER", "COPPER", "PLATINUM", "OIL" });
    }
}