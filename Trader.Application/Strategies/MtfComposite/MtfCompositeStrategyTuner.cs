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
        foreach (var tf in new[] { 1, 5, 15, 30, 60, 240, 1440, 10080 })
        {
            if (tf >= (int)primaryTf) allowedTrendTfs.Add(tf);
            if (tf < (int)primaryTf) allowedSubTfs.Add(tf);
        }

        return new ParameterDefinition[]
        {
        new IntParameter("EmaFastPeriod", 5, 100, 5,
            ParameterImportance.Important, "دوره EMA سریع", "دوره EMA سریع"),
        new IntParameter("EmaSlowPeriod", 20, 400, 20,
            ParameterImportance.Important, "دوره EMA کند — باید > Fast", "دوره EMA کند"),
        new TimeFrameParameter("TrendTimeFrame", allowedTrendTfs,
            ParameterImportance.Structural,
            "TF تحلیل روند — قبل از tuning fix کن", "تایم‌فریم روند"),
        new IntParameter("RsiPeriod", 7, 30, 1,
            ParameterImportance.Optional, "دوره RSI", "دوره RSI"),
        new DecimalParameter("RsiBullishThreshold", 50m, 75m, 2.5m,
            ParameterImportance.Important, "RSI بالای این → صعودی"),
        new DecimalParameter("RsiBearishThreshold", 25m, 50m, 2.5m,
            ParameterImportance.Important, "RSI زیر این → نزولی"),
        new IntParameter("MacdFast", 5, 20, 1,
            ParameterImportance.Optional, "دوره EMA سریع MACD"),
        new IntParameter("MacdSlow", 15, 60, 5,
            ParameterImportance.Optional, "دوره EMA کند MACD"),
        new IntParameter("MacdSignal", 5, 20, 1,
            ParameterImportance.Optional, "دوره signal EMA"),
        new IntParameter("AtrPeriod", 7, 30, 1,
            ParameterImportance.Optional, "دوره ATR"),
        new IntParameter("IntraDayBaselineCandles", 20, 500, 20,
            ParameterImportance.Noise, "کندل baseline حجم"),
        new IntParameter("OpeningBaselineDays", 5, 60, 5,
            ParameterImportance.Noise, "روز Opening baseline"),
        new IntParameter("MinBaselineSamples", 3, 30, 3,
            ParameterImportance.Noise, "حداقل نمونه baseline"),
        new DecimalParameter("VolumeMultiplier", 1.0m, 4.0m, 0.25m,
            ParameterImportance.Important, "چند برابر baseline = VolumeScore=1"),
        new DecimalParameter("BuyRatioThreshold", 0.9m, 3.0m, 0.1m,
            ParameterImportance.Noise, "آستانه BuyRatio"),
        new DecimalParameter("SellRatioThreshold", 0.9m, 3.0m, 0.1m,
            ParameterImportance.Noise, "آستانه SellRatio"),
        new DecimalParameter("OrderBookDepthRatio", 0.5m, 3.0m, 0.1m,
            ParameterImportance.Noise, "نسبت عمق Bid/Ask"),
        new DecimalParameter("TrendWeight", 0m, 1m, 0.05m,
            ParameterImportance.Important, "وزن لایه Trend"),
        new DecimalParameter("MomentumWeight", 0m, 1m, 0.05m,
            ParameterImportance.Important, "وزن لایه Momentum"),
        new DecimalParameter("MinSignalStrength", 0.3m, 0.95m, 0.05m,
            ParameterImportance.Critical, "حداقل قدرت سیگنال"),
        new DecimalParameter("MinStrengthDiff", 0.01m, 0.30m, 0.01m,
            ParameterImportance.Critical, "حداقل اختلاف Bull/Bear"),
        new DecimalParameter("AtrStopMultiplier", 0.5m, 6.0m, 0.5m,
            ParameterImportance.Critical, "فاصله SL = این × ATR"),
        new DecimalParameter("AtrTakeProfitMultiplier", 1.0m, 12.0m, 0.5m,
            ParameterImportance.Critical, "فاصله TP = این × ATR"),
        new BoolParameter("EnableIntraCandleDecision",
            ParameterImportance.Structural,
            "تصمیم intra-candle — قبل از tuning fix کن"),
        new TimeFrameParameter("SubCandleTimeFrame", allowedSubTfs,
            ParameterImportance.Structural,
            "TF sub-candle — قبل از tuning fix کن"),
        new IntParameter("SubCandleHistoryDepth", 200, 5000, 100,
            ParameterImportance.Noise, "تعداد کندل sub برای baseline"),
        new DecimalParameter("IntraCandleVolumeThreshold", 1.0m, 5.0m, 0.25m,
            ParameterImportance.Optional, "آستانه حجم intra-candle"),
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
        result["MicroWeight"] = 1m - tW - mW;
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