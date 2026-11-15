using FluentAssertions;
using Trader.Application.Indicators.Atr;
using Trader.Application.Runtime;
using Xunit;

namespace Trader.Tests.Application.Indicators;

public class AtrCalculatorTests
{
    private static Candle MakeCandle(decimal h, decimal l, decimal c) =>
        new(DateTime.UtcNow, c, h, l, c, 1000);

    [Fact]
    public void Calculate_NotEnoughData_ReturnsNull()
    {
        var candles = Enumerable.Range(1, 5)
            .Select(i => MakeCandle(10, 9, 9.5m)).ToList();

        var atr = AtrCalculator.Calculate(candles, period: 14);
        atr.Should().BeNull();
    }

    [Fact]
    public void Calculate_ConstantRange_ReturnsThatRange()
    {
        var candles = Enumerable.Range(1, 20)
            .Select(_ => MakeCandle(11, 9, 10)).ToList();

        var atr = AtrCalculator.Calculate(candles, period: 14);
        atr.Should().BeApproximately(2m, 0.001m);
    }

    [Fact]
    public void Calculate_OneCandle_ReturnsNull()
    {
        var candles = new[] { MakeCandle(11, 9, 10) };
        AtrCalculator.Calculate(candles, period: 14).Should().BeNull();
    }

    [Fact]
    public void Calculate_ExactPeriodPlusOne_ReturnsValidAtr()
    {
        var candles = Enumerable.Range(1, 15)
            .Select(_ => MakeCandle(11, 9, 10)).ToList();

        var atr = AtrCalculator.Calculate(candles, period: 14);
        atr.Should().NotBeNull();
        atr!.Value.Should().BeApproximately(2m, 0.001m);
    }
}