    using FluentAssertions;
    using global::Trader.Application.Indicators.Macd;
    using Xunit;

    namespace Trader.Tests.Application.Indicators;

    public class MacdCalculatorTests
    {
        [Fact]
        public void CalculateLast_NotEnoughData_ReturnsNull()
        {
            var values = Enumerable.Range(1, 20).Select(i => (decimal)i).ToList();
            var result = MacdCalculator.CalculateLast(values,
                new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
            result.Should().BeNull();
        }

        [Fact]
        public void CalculateLast_WithSufficientData_ReturnsNonNull()
        {
            var values = Enumerable.Range(1, 50).Select(i => (decimal)i).ToList();
            var result = MacdCalculator.CalculateLast(values,
                new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
            result.Should().NotBeNull();
        }

    [Fact]
    public void CalculateSeries_AcceleratingUptrend_PositiveHistogram()
    {
        // Accelerating rise → MACD line keeps rising → histogram stays positive.
        // (A linear uptrend would stabilize MACD at a constant, converging
        // histogram to zero.)
        var values = Enumerable.Range(1, 100)
            .Select(i => (decimal)i * (decimal)i)   // 1, 4, 9, ..., 10000
            .ToList();

        var (_, _, hist) = MacdCalculator.CalculateSeries(values,
            new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });

        hist[^1].Should().NotBeNull();
        hist[^1]!.Value.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void CalculateSeries_AcceleratingDowntrend_NegativeHistogram()
    {
        // In a *linear* downtrend, MACD line stabilizes at a constant
        // (both EMAs converge to the same slope), so histogram → 0.
        // To test "negative histogram during a downtrend", we need an
        // *accelerating* decline where MACD line keeps getting more negative.
        var values = Enumerable.Range(1, 100)
            .Select(i => 5000m - (decimal)i * (decimal)i)   // 4999, 4996, 4991, ..., -5000
            .ToList();

        var (_, _, hist) = MacdCalculator.CalculateSeries(values,
            new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });

        hist[^1].Should().NotBeNull();
        hist[^1]!.Value.Should().BeLessThan(0m);
    }

    [Fact]
        public void CalculateSeries_FastGreaterOrEqualSlow_Throws()
        {
            var values = Enumerable.Range(1, 100).Select(i => (decimal)i).ToList();
            var act = () => MacdCalculator.CalculateSeries(values,
                new MacdConfig { FastPeriod = 26, SlowPeriod = 26, SignalPeriod = 9 });
            act.Should().Throw<ArgumentException>();
        }
    }