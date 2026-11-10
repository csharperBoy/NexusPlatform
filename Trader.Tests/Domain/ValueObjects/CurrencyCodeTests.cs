using FluentAssertions;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Domain;

public class CurrencyCodeTests
{
    [Theory]
    [InlineData("USD")]
    [InlineData("IRR")]
    [InlineData("GOLD")]
    [InlineData("SILVER")]
    [InlineData("COPPER")]
    [InlineData("PLATINUM")]
    [InlineData("OIL")]
    public void RequireValid_AcceptsWhitelisted(string code)
    {
        var result = CurrencyCode.RequireValid(code, "targetCurrency");
        result.Should().Be(code.ToUpperInvariant());
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("gold", "GOLD")]
    [InlineData("  PlAtInUm  ", "PLATINUM")]
    public void RequireValid_NormalizesCaseAndWhitespace(string input, string expected)
    {
        CurrencyCode.RequireValid(input, "targetCurrency").Should().Be(expected);
    }

    [Theory]
    [InlineData("USDD")]
    [InlineData("XAU")]
    [InlineData("")]
    [InlineData("A")]
    public void RequireValid_RejectsInvalid(string code)
    {
        Action act = () => CurrencyCode.RequireValid(code, "targetCurrency");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeOrNull_ReturnsNullForEmpty(string? code)
    {
        CurrencyCode.NormalizeOrNull(code, "targetCurrency").Should().BeNull();
    }

    [Fact]
    public void NormalizeOrNull_ValidatesWhenProvided()
    {
        CurrencyCode.NormalizeOrNull("gold", "x").Should().Be("GOLD");

        Action act = () => CurrencyCode.NormalizeOrNull("XAU", "x");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void All_ContainsBothFiatAndCommodities()
    {
        var all = CurrencyCode.All;
        all.Should().Contain(new[] { "IRR", "USD", "EUR" });
        all.Should().Contain(new[] { "GOLD", "SILVER", "COPPER", "PLATINUM", "OIL" });
    }
}