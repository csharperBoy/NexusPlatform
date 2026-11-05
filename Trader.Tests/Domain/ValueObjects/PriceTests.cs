using FluentAssertions;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Domain.ValueObjects;

public class PriceTests
{
    [Fact]
    public void Constructor_WithPositiveValue_SetsValue()
    {
        var price = new Price(1500.5m);
        price.Value.Should().Be(1500.5m);
    }

    [Fact]
    public void Constructor_WithZero_IsAllowed()
    {
        var price = new Price(0m);
        price.Value.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithNegativeValue_Throws()
    {
        var act = () => new Price(-1m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Zero_ReturnsZeroValue()
    {
        Price.Zero.Value.Should().Be(0m);
    }

    [Fact]
    public void ComparisonOperators_WorkCorrectly()
    {
        var a = new Price(100m);
        var b = new Price(200m);

        (a < b).Should().BeTrue();
        (a > b).Should().BeFalse();
        (a <= b).Should().BeTrue();
        (a >= b).Should().BeFalse();
    }

    [Fact]
    public void ToString_FormatsWithUpToFourDecimals()
    {
        new Price(1234.56789m).ToString().Should().Be("1234.5679");
    }
}