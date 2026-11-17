// Trader.Application/Strategies/MtfComposite/MtfCompositeStrategyTuner.cs
using System.Text.Json.Nodes;
using Trader.Application.Strategies.Tuning;
using Trader.Domain.Enums;

namespace Trader.Application.Strategies.MtfComposite;

public sealed class MtfCompositeStrategyTuner : IStrategyTuner
{
    public string TunerKey => "mtf-composite";

    public IReadOnlyList<ParameterDefinition> GetParameterSpace(TimeFrame primaryTf)
    {
        var allowedTrendTfs = new List<int>();
        var allowedSubTfs = new List<int>();
        foreach (var tf in new[] { (int)TimeFrame.M1, (int)TimeFrame.M5,
                                    (int)TimeFrame.M15, (int)TimeFrame.M30,
                                    (int)TimeFrame.H1, (int)TimeFrame.H4,
                                    (int)TimeFrame.D1, (int)TimeFrame.W1 })
        {
            if (tf >= (int)primaryTf) allowedTrendTfs.Add(tf);
            if (tf < (int)primaryTf) allowedSubTfs.Add(tf);
        }

        return new ParameterDefinition[]
        {
            new IntParameter("EmaFastPeriod", 5, 100, 5),
            new IntParameter("EmaSlowPeriod", 20, 400, 20),
            new TimeFrameParameter("TrendTimeFrame", allowedTrendTfs),
            new IntParameter("RsiPeriod", 7, 30, 1),
            new DecimalParameter("RsiBullishThreshold", 50m, 75m, 2.5m),
            new DecimalParameter("RsiBearishThreshold", 25m, 50m, 2.5m),
            new IntParameter("MacdFast", 5, 20, 1),
            new IntParameter("MacdSlow", 15, 60, 5),
            new IntParameter("MacdSignal", 5, 20, 1),
            new IntParameter("AtrPeriod", 7, 30, 1),
            new IntParameter("IntraDayBaselineCandles", 20, 500, 20),
            new IntParameter("OpeningBaselineDays", 5, 60, 5),
            new IntParameter("MinBaselineSamples", 3, 30, 3),
            new DecimalParameter("VolumeMultiplier", 1.0m, 4.0m, 0.25m),
            new DecimalParameter("BuyRatioThreshold", 0.9m, 3.0m, 0.1m),
            new DecimalParameter("SellRatioThreshold", 0.9m, 3.0m, 0.1m),
            new DecimalParameter("OrderBookDepthRatio", 0.5m, 3.0m, 0.1m),
            new DecimalParameter("TrendWeight", 0m, 1m, 0.05m),
            new DecimalParameter("MomentumWeight", 0m, 1m, 0.05m),
            new DecimalParameter("MinSignalStrength", 0.3m, 0.95m, 0.05m),
            new DecimalParameter("MinStrengthDiff", 0.01m, 0.30m, 0.01m),
            new DecimalParameter("AtrStopMultiplier", 0.5m, 6.0m, 0.5m),
            new DecimalParameter("AtrTakeProfitMultiplier", 1.0m, 12.0m, 0.5m),
            new BoolParameter("EnableIntraCandleDecision"),
            new TimeFrameParameter("SubCandleTimeFrame", allowedSubTfs),
            new IntParameter("SubCandleHistoryDepth", 200, 5000, 100),
            new DecimalParameter("IntraCandleVolumeThreshold", 1.0m, 5.0m, 0.25m),
        };
    }

    public string? ValidateConfiguration(JsonObject c, TimeFrame primaryTf)
    {
        var fast = c["EmaFastPeriod"]?.GetValue<int>() ?? 20;
        var slow = c["EmaSlowPeriod"]?.GetValue<int>() ?? 50;
        if (fast >= slow) return "EmaFastPeriod must be < EmaSlowPeriod";

        var macdFast = c["MacdFast"]?.GetValue<int>() ?? 12;
        var macdSlow = c["MacdSlow"]?.GetValue<int>() ?? 26;
        if (macdFast >= macdSlow) return "MacdFast must be < MacdSlow";

        var bull = c["RsiBullishThreshold"]?.GetValue<decimal>() ?? 55m;
        var bear = c["RsiBearishThreshold"]?.GetValue<decimal>() ?? 45m;
        if (bear >= bull) return "RsiBearishThreshold must be < RsiBullishThreshold";

        var tW = c["TrendWeight"]?.GetValue<decimal>() ?? 0.4m;
        var mW = c["MomentumWeight"]?.GetValue<decimal>() ?? 0.35m;
        var microW = 1m - tW - mW;
        if (microW < 0m || microW > 1m) return "Weights must sum to 1.0";

        return null;
    }

    public JsonObject RepairConstraints(JsonObject c, TimeFrame primaryTf)
    {
        var result = (JsonObject)c.DeepClone();

        if ((result["EmaFastPeriod"]?.GetValue<int>() ?? 20)
            >= (result["EmaSlowPeriod"]?.GetValue<int>() ?? 50))
            result["EmaSlowPeriod"] = (result["EmaFastPeriod"]?.GetValue<int>() ?? 20) + 10;

        if ((result["MacdFast"]?.GetValue<int>() ?? 12)
            >= (result["MacdSlow"]?.GetValue<int>() ?? 26))
            result["MacdSlow"] = (result["MacdFast"]?.GetValue<int>() ?? 12) + 5;

        if ((result["RsiBearishThreshold"]?.GetValue<decimal>() ?? 45m)
            >= (result["RsiBullishThreshold"]?.GetValue<decimal>() ?? 55m))
            result["RsiBullishThreshold"] = (result["RsiBearishThreshold"]?.GetValue<decimal>() ?? 45m) + 5m;

        // Weights
        var tW = result["TrendWeight"]?.GetValue<decimal>() ?? 0.4m;
        var mW = result["MomentumWeight"]?.GetValue<decimal>() ?? 0.35m;
        if (tW + mW > 1m)
        {
            var scale = 1m / (tW + mW);
            tW *= scale; mW *= scale;
            result["TrendWeight"] = tW;
            result["MomentumWeight"] = mW;
        }

        return result;
    }

    public JsonObject GetDefaults(TimeFrame primaryTf)
    {
        var cfg = new MtfCompositeStrategyConfig();
        return new JsonObject
        {
            ["EmaFastPeriod"] = cfg.EmaFastPeriod,
            ["EmaSlowPeriod"] = cfg.EmaSlowPeriod,
            ["TrendTimeFrame"] = (int)cfg.TrendTimeFrame,
            ["RsiPeriod"] = cfg.RsiPeriod,
            ["RsiBullishThreshold"] = cfg.RsiBullishThreshold,
            ["RsiBearishThreshold"] = cfg.RsiBearishThreshold,
            ["MacdFast"] = cfg.MacdFast,
            ["MacdSlow"] = cfg.MacdSlow,
            ["MacdSignal"] = cfg.MacdSignal,
            ["AtrPeriod"] = cfg.AtrPeriod,
            ["IntraDayBaselineCandles"] = cfg.IntraDayBaselineCandles,
            ["OpeningBaselineDays"] = cfg.OpeningBaselineDays,
            ["MinBaselineSamples"] = cfg.MinBaselineSamples,
            ["VolumeMultiplier"] = cfg.VolumeMultiplier,
            ["BuyRatioThreshold"] = cfg.BuyRatioThreshold,
            ["SellRatioThreshold"] = cfg.SellRatioThreshold,
            ["OrderBookDepthRatio"] = cfg.OrderBookDepthRatio,
            ["TrendWeight"] = cfg.TrendWeight,
            ["MomentumWeight"] = cfg.MomentumWeight,
            ["MicroWeight"] = cfg.MicroWeight,
            ["MinSignalStrength"] = cfg.MinSignalStrength,
            ["MinStrengthDiff"] = cfg.MinStrengthDiff,
            ["AtrStopMultiplier"] = cfg.AtrStopMultiplier,
            ["AtrTakeProfitMultiplier"] = cfg.AtrTakeProfitMultiplier,
            ["EnableIntraCandleDecision"] = cfg.EnableIntraCandleDecision,
            ["SubCandleTimeFrame"] = (int)cfg.SubCandleTimeFrame,
            ["SubCandleHistoryDepth"] = cfg.SubCandleHistoryDepth,
            ["IntraCandleVolumeThreshold"] = cfg.IntraCandleVolumeThreshold,
        };
    }
}