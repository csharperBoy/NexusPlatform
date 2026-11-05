using FluentAssertions;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Rsi;
using Trader.Application.Runtime;
using Trader.Application.Strategies.Rsi;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Application.Strategies;

public class RsiStrategyTests
{
    [Fact]
    public void Evaluate_WithOversoldRsi_GeneratesBuySignal()
    {
        // 40 کندل با روند نزولی یکنواخت → RSI = 0 (oversold)
        var closes = Enumerable.Range(0, 40).Select(i => 100m - i).ToList();
        var sut = CreateStrategy(minSignalStrength: 0m);
        var input = BuildInput(closes);

        var result = sut.Evaluate(input);

        result.Signals.Should().HaveCount(1);
        result.Signals[0].Action.Should().Be(SignalAction.Buy);
        result.Signals[0].Isin.Should().Be("TEST");
        result.Signals[0].Strength.Value.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void Evaluate_WithOverboughtRsi_GeneratesSellSignal()
    {
        // 40 کندل با روند صعودی یکنواخت → RSI = 100 (overbought)
        var closes = Enumerable.Range(0, 40).Select(i => 60m + i).ToList();
        var sut = CreateStrategy(minSignalStrength: 0m);
        var input = BuildInput(closes);

        var result = sut.Evaluate(input);

        result.Signals.Should().HaveCount(1);
        result.Signals[0].Action.Should().Be(SignalAction.Sell);
    }

    [Fact]
    public void Evaluate_WithNeutralRsi_GeneratesNoSignal()
    {
        // 40 کندل با قیمت ثابت → RSI = 50 (neutral)
        var closes = Enumerable.Repeat(100m, 40).ToList();
        var sut = CreateStrategy(minSignalStrength: 0m);
        var input = BuildInput(closes);

        var result = sut.Evaluate(input);

        result.Signals.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WithInsufficientCandles_ReturnsEmpty()
    {
        // 3 کندل — کمتر از period+1 = 15 → RSI قابل محاسبه نیست
        var closes = new[] { 100m, 101m, 102m };
        var sut = CreateStrategy(minSignalStrength: 0m);
        var input = BuildInput(closes);

        var result = sut.Evaluate(input);

        result.Signals.Should().BeEmpty();
        result.Features.Should().BeNull();
    }

    [Fact]
    public void Evaluate_WhenMinSignalStrengthNotMet_SuppressesSignal()
    {
        // روند نزولی یکنواخت → RSI = 0، strength = (30-0)/30 = 1.0
        // با MinSignalStrength = 1.5 (بزرگ‌تر از حداکثر ممکن)، هیچ سیگنالی نباید صادر شود
        var closes = Enumerable.Range(0, 40).Select(i => 100m - i).ToList();
        var sut = CreateStrategy(minSignalStrength: 1.01m);   // > 1.0
        var input = BuildInput(closes);

        var result = sut.Evaluate(input);

        result.Signals.Should().BeEmpty(
            because: "strength=1.0 is below MinSignalStrength=1.01");
        result.Features.Should().NotBeNull(
            because: "features are returned even when no signal");
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private static RsiStrategy CreateStrategy(decimal minSignalStrength)
    {
        var config = new RsiStrategyConfig
        {
            Period = 14,
            OversoldThreshold = 30m,
            OverboughtThreshold = 70m,
            MinSignalStrength = minSignalStrength,
        };
        return new RsiStrategy(new RsiIndicator(), config);
    }

    private static StrategyInput BuildInput(IReadOnlyList<decimal> closes)
    {
        var candles = new List<Candle>(closes.Count);
        var time = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        foreach (var close in closes)
        {
            candles.Add(new Candle(
                OpenTime: time,
                Open: close,
                High: close + 1m,
                Low: close - 1m,
                Close: close,
                Volume: 1000));
            time = time.AddDays(1);
        }

        return new StrategyInput(
            Isin: "TEST",
            LatestSnapshot: null,
            Candles: candles,
            ExtraTimeFrames: null,
            Configuration: new RsiStrategyConfig { Period = 14 },
            TimeFrame: TimeFrame.D1,
            EvaluatedAt: time);
    }
}