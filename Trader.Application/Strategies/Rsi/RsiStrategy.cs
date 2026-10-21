using Trader.Application.Abstractions.Strategies;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Strategies.Rsi;

public sealed class RsiStrategy : IStrategy
{
    private readonly RsiStrategyConfig _config;

    public RsiStrategy(RsiStrategyConfig config) => _config = config;

    public string Key => RsiStrategyRegistration.KeyName;
    public string DisplayName => RsiStrategyRegistration.DisplayNameValue;
    public int RequiredCandleCount => _config.Period + 50;

    public StrategyEvaluationResult Evaluate(StrategyInput input)
    {
        if (input.Candles.Count < _config.Period + 1)
            return StrategyEvaluationResult.Empty();  // no features, no signal

        var closes = input.Candles.Select(c => c.Close).ToList();
        var rsi = RsiCalculator.Calculate(closes, _config.Period);
        if (rsi is null)
            return StrategyEvaluationResult.Empty();

        var value = rsi.Value;

        // ─── Feature Vector ───
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

        // ─── BUY ───
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
        // ─── SELL ───
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