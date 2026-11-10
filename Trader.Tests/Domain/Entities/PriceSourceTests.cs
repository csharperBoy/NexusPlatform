using FluentAssertions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class PriceSourceTests
{
    [Fact]
    public void Create_NormalizesAssetsToUppercase()
    {
        var s = PriceSource.Create(
            "tgju-usd", "TGJU", PriceSourceType.Tgju,
            """{"indicator":"price_dollar_rl"}""",
            fromAsset: "usd", toAsset: "irr");

        s.FromAsset.Should().Be("USD");
        s.ToAsset.Should().Be("IRR");
    }

    [Fact]
    public void Create_NormalizesCodeToLowercase()
    {
        var s = PriceSource.Create(
            "TGJU-USD", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        s.Code.Should().Be("tgju-usd");
    }

    [Fact]
    public void Create_RejectsSamePair()
    {
        var act = () => PriceSource.Create(
            "x", "X", PriceSourceType.Tgju, "{}", "USD", "usd");
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsEmptyCode(string code)
    {
        var act = () => PriceSource.Create(
            code, "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var s = PriceSource.Create(
            "x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        s.Deactivate();
        s.IsActive.Should().BeFalse();
    }
}