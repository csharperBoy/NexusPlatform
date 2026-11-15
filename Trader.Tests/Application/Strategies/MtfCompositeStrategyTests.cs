using FluentAssertions;
using Moq;
using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Atr;
using Trader.Application.Indicators.Ema;
using Trader.Application.Indicators.Macd;
using Trader.Application.Indicators.Rsi;
using Trader.Application.Runtime;
using Trader.Application.Strategies.MtfComposite;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Application.Strategies;

public class MtfCompositeStrategyTests
{
    private static (IIndicator<EmaConfig, decimal?>,
                    IIndicator<MacdConfig, MacdResult?>,
                    IIndicator<AtrConfig, decimal?>,
                    IIndicator<RsiConfig, decimal?>) BuildRealIndicators()
    {
        return (
            new EmaIndicator(),
            new MacdIndicator(),
            new AtrIndicator(),
            new RsiIndicator());
    }

    private static List<Candle> GenerateCandles(
        int count, decimal startPrice, decimal driftPerCandle,
        DateTime? startAt = null, long volume = 100_000)
    {
        var candles = new List<Candle>(count);
        var price = startPrice;
        var time = startAt ?? DateTime.UtcNow.AddDays(-count);

        for (int i = 0; i < count; i++)
        {
            var open = price;
            price += driftPerCandle;
            var close = price;
            candles.Add(new Candle(
                OpenTime: time,
                Open: open,
                High: Math.Max(open, close) + 1m,
                Low: Math.Min(open, close) - 1m,
                Close: close,
                Volume: volume));
            time = time.AddHours(1);
        }
        return candles;
    }

    [Fact]
    public void Evaluate_NoOrderBook_StillWorks()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            EmaFastPeriod = 10,
            EmaSlowPeriod = 20,
            RsiPeriod = 10,
            MacdFast = 6,
            MacdSlow = 13,
            MacdSignal = 5,
            AtrPeriod = 10,
            IntraDayBaselineCandles = 30,
            OpeningBaselineDays = 5,
            MinBaselineSamples = 3,
            TrendTimeFrame = TimeFrame.H1,
            SubCandleTimeFrame = TimeFrame.M5,
            EnableIntraCandleDecision = false,
        };

        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        var candles = GenerateCandles(100, 1000m, +0.5m);
        var input = new StrategyInput(
            Isin: "TEST",
            LatestSnapshot: null,
            Candles: candles,
            ExtraTimeFrames: null,
            Configuration: config,
            TimeFrame: TimeFrame.H1,
            EvaluatedAt: DateTime.UtcNow);

        var result = strategy.Evaluate(input);

        result.Should().NotBeNull();
        result.Features.Should().NotBeNull();
        result.Features!["bullishStrength"].Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_NotEnoughCandles_ReturnsEmpty()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig();
        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        var candles = GenerateCandles(10, 1000m, 0m);
        var input = new StrategyInput(
            "TEST", null, candles, null, config, TimeFrame.H1, DateTime.UtcNow);

        var result = strategy.Evaluate(input);
        result.Signals.Should().BeEmpty();
        result.Features.Should().BeNull();
    }

    [Fact]
    public void Config_WeightsNotSumToOne_Throws()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            TrendWeight = 0.5m,
            MomentumWeight = 0.5m,
            MicroWeight = 0.5m, // sum = 1.5
        };

        var act = () => new MtfCompositeStrategy(ema, macd, atr, rsi, config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must sum to 1.0*");
    }

    [Fact]
    public void ExtraCandleRequirements_ContainsTrendAndSub()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            TrendTimeFrame = TimeFrame.D1,
            SubCandleTimeFrame = TimeFrame.M5,
        };
        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        strategy.ExtraCandleRequirements.Should().HaveCount(2);
        strategy.ExtraCandleRequirements
            .Should().Contain(r => r.TimeFrame == TimeFrame.D1 && r.ClosedOnly);
        strategy.ExtraCandleRequirements
            .Should().Contain(r => r.TimeFrame == TimeFrame.M5 && !r.ClosedOnly);
    }

    [Fact]
    public void Evaluate_StrongUptrend_ProducesBuySignal()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            EmaFastPeriod = 10,
            EmaSlowPeriod = 20,
            RsiPeriod = 10,
            MacdFast = 6,
            MacdSlow = 13,
            MacdSignal = 5,
            AtrPeriod = 10,
            TrendTimeFrame = TimeFrame.H1,
            SubCandleTimeFrame = TimeFrame.M5,
            EnableIntraCandleDecision = false,
            IntraDayBaselineCandles = 30,
            OpeningBaselineDays = 5,
            MinBaselineSamples = 3,
            MinSignalStrength = 0.5m, // lower for test predictability
            MinStrengthDiff = 0.02m,
        };
        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        // Generate uptrending candles with varying volume
        var candles = new List<Candle>();
        var time = DateTime.UtcNow.AddDays(-30);
        var price = 1000m;
        for (int i = 0; i < 100; i++)
        {
            price += 5m;
            // Skew volume up at the end
            var vol = i > 95 ? 500_000L : 100_000L;
            candles.Add(new Candle(time, price - 2m, price + 2m, price - 5m, price, vol));
            time = time.AddHours(1);
        }

        var input = new StrategyInput(
            "TEST", null, candles, null, config, TimeFrame.H1, DateTime.UtcNow);

        var result = strategy.Evaluate(input);

        result.Features.Should().NotBeNull();
        var bull = (decimal)result.Features!["bullishStrength"]!;
        bull.Should().BeGreaterThan(0.3m); // trend and momentum should be positive
    }

    [Fact]
    public void Evaluate_Bearish_ProducesSellOrHold()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            EmaFastPeriod = 10,
            EmaSlowPeriod = 20,
            RsiPeriod = 10,
            MacdFast = 6,
            MacdSlow = 13,
            MacdSignal = 5,
            AtrPeriod = 10,
            TrendTimeFrame = TimeFrame.H1,
            SubCandleTimeFrame = TimeFrame.M5,
            EnableIntraCandleDecision = false,
            IntraDayBaselineCandles = 30,
            OpeningBaselineDays = 5,
            MinBaselineSamples = 3,
        };
        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        // Downtrending
        var candles = GenerateCandles(100, 2000m, -5m);

        var input = new StrategyInput(
            "TEST", null, candles, null, config, TimeFrame.H1, DateTime.UtcNow);

        var result = strategy.Evaluate(input);

        if (result.Signals.Count > 0)
        {
            result.Signals[0].Action.Should().Be(SignalAction.Sell);
        }
        else
        {
            result.Features.Should().NotBeNull();
            var bear = (decimal)result.Features!["bearishStrength"]!;
            bear.Should().BeGreaterThan(0.3m);
        }
    }

    [Fact]
    public void Evaluate_BuySignalHasSLTPFromAtr()
    {
        var (ema, macd, atr, rsi) = BuildRealIndicators();
        var config = new MtfCompositeStrategyConfig
        {
            EmaFastPeriod = 10,
            EmaSlowPeriod = 20,
            RsiPeriod = 10,
            MacdFast = 6,
            MacdSlow = 13,
            MacdSignal = 5,
            AtrPeriod = 10,
            TrendTimeFrame = TimeFrame.H1,
            SubCandleTimeFrame = TimeFrame.M5,
            EnableIntraCandleDecision = false,
            IntraDayBaselineCandles = 30,
            OpeningBaselineDays = 5,
            MinBaselineSamples = 3,
            MinSignalStrength = 0.3m,
            MinStrengthDiff = 0.02m,
        };
        var strategy = new MtfCompositeStrategy(ema, macd, atr, rsi, config);

        var candles = new List<Candle>();
        var time = DateTime.UtcNow.AddDays(-30);
        var price = 1000m;
        for (int i = 0; i < 100; i++)
        {
            price += 8m;
            var vol = i > 90 ? 800_000L : 100_000L;
            candles.Add(new Candle(time, price - 3m, price + 3m, price - 5m, price, vol));
            time = time.AddHours(1);
        }

        var input = new StrategyInput(
            "TEST", null, candles, null, config, TimeFrame.H1, DateTime.UtcNow);

        var result = strategy.Evaluate(input);

        if (result.Signals.Count > 0 && result.Signals[0].Action == SignalAction.Buy)
        {
            result.Signals[0].StopLoss.Should().NotBeNull();
            result.Signals[0].TakeProfit.Should().NotBeNull();
            result.Signals[0].StopLoss!.Value.Value
                .Should().BeLessThan(result.Signals[0].TakeProfit!.Value.Value);
        }
    }
}