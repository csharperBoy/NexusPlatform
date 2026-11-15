using FluentAssertions;
using Trader.Application.Indicators.Ema;
using Xunit;

namespace Trader.Tests.Application.Indicators;

public class EmaCalculatorTests
{
    [Fact]
    public void CalculateSeries_NotEnoughData_ReturnsAllNull()
    {
        var values = new List<decimal> { 1, 2, 3 };
        var series = EmaCalculator.CalculateSeries(values, period: 5);

        series.Should().HaveCount(3);
        series.Should().AllSatisfy(v => v.Should().BeNull());
    }

    [Fact]
    public void CalculateSeries_SeedIsSma()
    {
        var values = new List<decimal> { 1, 2, 3, 4, 5 };
        var series = EmaCalculator.CalculateSeries(values, period: 5);

        series.Should().HaveCount(5);
        series.Take(4).Should().AllSatisfy(v => v.Should().BeNull());
        series[4].Should().Be(3m); // SMA(1..5)
    }

    [Fact]
    public void CalculateSeries_RecursiveCorrect()
    {
        var values = new List<decimal> { 10, 20, 30, 40, 50, 60 };
        var series = EmaCalculator.CalculateSeries(values, period: 3);

        // Seed (idx 2) = SMA(10,20,30) = 20
        series[2].Should().Be(20m);

        // k = 2/4 = 0.5
        // EMA[3] = (40 - 20)*0.5 + 20 = 30
        series[3].Should().Be(30m);

        // EMA[4] = (50 - 30)*0.5 + 30 = 40
        series[4].Should().Be(40m);

        // EMA[5] = (60 - 40)*0.5 + 40 = 50
        series[5].Should().Be(50m);
    }

    [Fact]
    public void CalculateLast_ReturnsLastValue()
    {
        var values = new List<decimal> { 1, 2, 3, 4, 5 };
        var last = EmaCalculator.CalculateLast(values, period: 5);
        last.Should().Be(3m);
    }
}