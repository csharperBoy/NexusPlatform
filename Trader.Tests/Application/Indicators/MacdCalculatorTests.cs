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
        public void CalculateSeries_Uptrend_PositiveHistogram()
        {
            var values = Enumerable.Range(1, 100).Select(i => (decimal)i).ToList();
            var (_, _, hist) = MacdCalculator.CalculateSeries(values,
                new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
            hist[^1].Should().BeGreaterThan(0m);
        }

        [Fact]
        public void CalculateSeries_Downtrend_NegativeHistogram()
        {
            var values = Enumerable.Range(1, 100).Reverse().Select(i => (decimal)i).ToList();
            var (_, _, hist) = MacdCalculator.CalculateSeries(values,
                new MacdConfig { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
            hist[^1].Should().BeLessThan(0m);
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