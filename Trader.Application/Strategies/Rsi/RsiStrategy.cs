using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Rsi;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Strategies.Rsi;

public sealed class RsiStrategy : IStrategy
{
    private readonly IIndicator<RsiConfig, decimal?> _rsiIndicator;
    private readonly RsiStrategyConfig _config;

    public RsiStrategy(
        IIndicator<RsiConfig, decimal?> rsiIndicator,
        RsiStrategyConfig config)
    {
        _rsiIndicator = rsiIndicator;
        _config = config;
    }

    public string Key => RsiStrategyRegistration.KeyName;
    public string DisplayName => RsiStrategyRegistration.DisplayNameValue;
    public int RequiredCandleCount => _config.Period + 50;

    public bool RequiresClosedCandlesOnly => true;
    public StrategyEvaluationResult Evaluate(StrategyInput input)
    {
        // ─── ۱. Indicator (فقط محاسبه) ───
        var rsiConfig = new RsiConfig { Period = _config.Period };

        var rsi = _rsiIndicator.Compute(
            new IndicatorInput(
                input.Candles,
                input.LatestSnapshot,
                input.TimeFrame),
            rsiConfig);

        if (rsi is null)
            return StrategyEvaluationResult.Empty();

        var value = rsi.Value;

        // ─── ۲. Features (برای AI/ML) ───
        var features = new Dictionary<string, object?>
        {
            ["rsi"] = value,
            ["period"] = _config.Period,
            ["oversoldThreshold"] = _config.OversoldThreshold,
            ["overboughtThreshold"] = _config.OverboughtThreshold,
            ["minSignalStrength"] = _config.MinSignalStrength,
            ["candleCount"] = input.Candles.Count,
            ["lastClose"] = input.Candles[^1].Close,
            ["lastCandleTime"] = input.Candles[^1].OpenTime,
        };

        // ─── ۳. تصمیم (منطق استراتژی) ───
        if (value < _config.OversoldThreshold)
        {
            var strength = ComputeStrength(value, _config.OversoldThreshold, isBuy: true);
            if (strength.Value >= _config.MinSignalStrength)
            {
                return new StrategyEvaluationResult(
                    new[]
                    {
                        new StrategySignal(
                            input.Isin,
                            SignalAction.Buy,
                            strength,
                            MetadataJson: $"{{\"rsi\":{value:0.##}}}")
                    },
                    features);
            }
        }
        else if (value > _config.OverboughtThreshold)
        {
            var strength = ComputeStrength(value, _config.OverboughtThreshold, isBuy: false);
            if (strength.Value >= _config.MinSignalStrength)
            {
                return new StrategyEvaluationResult(
                    new[]
                    {
                        new StrategySignal(
                            input.Isin,
                            SignalAction.Sell,
                            strength,
                            MetadataJson: $"{{\"rsi\":{value:0.##}}}")
                    },
                    features);
            }
        }

        // ─── Hold (با features برای Dataset) ───
        return StrategyEvaluationResult.Empty(features);
    }

    private static SignalStrength ComputeStrength(
        decimal rsi, decimal threshold, bool isBuy)
    {
        decimal raw = isBuy
            ? (threshold - rsi) / threshold
            : (rsi - threshold) / (100m - threshold);

        if (raw < 0m) raw = 0m;
        if (raw > 1m) raw = 1m;

        return new SignalStrength(raw);
    }
}