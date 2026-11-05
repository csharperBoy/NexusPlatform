using FluentAssertions;
using Trader.Application.Indicators.Rsi;
using Xunit;

namespace Trader.Tests.Application.Indicators;

public class RsiCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNull()
    {
        var closes = new[] { 1m, 2m, 3m };
        RsiCalculator.Calculate(closes, period: 14).Should().BeNull();
    }

    [Fact]
    public void Calculate_WithAllGains_Returns100()
    {
        var closes = Enumerable.Range(1, 20).Select(i => (decimal)i).ToList();
        var rsi = RsiCalculator.Calculate(closes, period: 14);
        rsi.Should().Be(100m);
    }

    [Fact]
    public void Calculate_WithAllLosses_Returns0()
    {
        var closes = Enumerable.Range(1, 20).Reverse().Select(i => (decimal)i).ToList();
        var rsi = RsiCalculator.Calculate(closes, period: 14);
        rsi.Should().Be(0m);
    }

    [Fact]
    public void Calculate_WithFlatPrices_Returns50()
    {
        var closes = Enumerable.Repeat(100m, 20).ToList();
        var rsi = RsiCalculator.Calculate(closes, period: 14);
        rsi.Should().Be(50m);
    }

    [Fact]
    public void Calculate_WithKnownReferenceSeries_MatchesExpected()
    {
        // Wilder's classic RSI(14) — اولین RSI معتبر پس از ۱۴ تغییر
        // مقدار مرجع: 70.46 (Wilder 1978, StockCharts)
        //
        // نکته: با ۱۵ close، حساب‌گر فقط از Simple Average استفاده می‌کند
        // (چون looping smoothing برای index > 14 اجرا نمی‌شود). مقدار 70.46
        // در ادبیات مالی برای این سری مستند است.
        var closes = new[]
        {
        44.34m, 44.09m, 44.15m, 43.61m, 44.33m, 44.83m, 45.10m, 45.42m,
        45.84m, 46.08m, 45.89m, 46.03m, 45.61m, 46.28m, 46.28m
    };

        var rsi = RsiCalculator.Calculate(closes, period: 14);

        rsi.Should().NotBeNull();
        rsi!.Value.Should().BeApproximately(70.46m, 0.1m);
    }
    [Fact]
    public void Calculate_WithWilderSmoothing_FullSeries_MatchesReference()
    {
        // Wilder's classic 33-value series — RSI نهایی بعد از اعمال Smoothing
        // مقدار مرجع: ~37.79 (پایان سری پس از ۱۸ مرحله Wilder Smoothing)
        //
        // این تست صحت Wilder Smoothing را تأیید می‌کند. تلورانس 0.5
        // برای پوشش اختلاف گرد‌کردن Decimal در پیاده‌سازی‌های مختلف.
        var closes = new[]
        {
        44.34m, 44.09m, 44.15m, 43.61m, 44.33m, 44.83m, 45.10m, 45.42m,
        45.84m, 46.08m, 45.89m, 46.03m, 45.61m, 46.28m, 46.28m, 46.00m,
        46.03m, 46.41m, 46.22m, 45.64m, 46.21m, 46.25m, 45.71m, 46.45m,
        45.78m, 45.35m, 44.03m, 44.18m, 44.22m, 44.57m, 43.42m, 42.66m,
        43.13m
    };

        var rsi = RsiCalculator.Calculate(closes, period: 14);

        rsi.Should().NotBeNull();
        rsi!.Value.Should().BeApproximately(37.79m, 0.5m);
    }
}