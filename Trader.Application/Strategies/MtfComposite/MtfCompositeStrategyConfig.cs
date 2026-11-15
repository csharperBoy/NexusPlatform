using Trader.Application.Abstractions.Strategies;
using Trader.Domain.Enums;

namespace Trader.Application.Strategies.MtfComposite;

public sealed class MtfCompositeStrategyConfig : StrategyConfig
{
    public override string StrategyKey => MtfCompositeStrategyRegistration.KeyName;

    // ─── Trend Layer (EMA on TrendTimeFrame) ───
    public int EmaFastPeriod { get; set; } = 20;
    public int EmaSlowPeriod { get; set; } = 50;

    /// <summary>
    /// TF برای تحلیل روند. باید >= PrimaryTimeFrame باشه (روی داده‌ی طولانی‌تر).
    /// پیش‌فرض D1. اگه = PrimaryTimeFrame باشه، از همون کندل‌های primary استفاده می‌شه.
    /// </summary>
    public TimeFrame TrendTimeFrame { get; set; } = TimeFrame.D1;

    // ─── Momentum Layer ───
    public int RsiPeriod { get; set; } = 14;
    public decimal RsiBullishThreshold { get; set; } = 55m;
    public decimal RsiBearishThreshold { get; set; } = 45m;
    public int MacdFast { get; set; } = 12;
    public int MacdSlow { get; set; } = 26;
    public int MacdSignal { get; set; } = 9;

    // ─── Volatility ───
    public int AtrPeriod { get; set; } = 14;

    // ─── Volume Baselines ───
    public int IntraDayBaselineCandles { get; set; } = 100;
    public int OpeningBaselineDays { get; set; } = 30;
    public int MinBaselineSamples { get; set; } = 10;
    public decimal VolumeMultiplier { get; set; } = 1.5m;

    // ─── Order Book ───
    public decimal BuyRatioThreshold { get; set; } = 1.1m;
    public decimal SellRatioThreshold { get; set; } = 1.1m;
    public decimal OrderBookDepthRatio { get; set; } = 1.5m;

    // ─── Weights (باید جمع = 1.0) ───
    public decimal TrendWeight { get; set; } = 0.40m;
    public decimal MomentumWeight { get; set; } = 0.35m;
    public decimal MicroWeight { get; set; } = 0.25m;

    // ─── Signal ───
    public decimal MinSignalStrength { get; set; } = 0.65m;
    public decimal MinStrengthDiff { get; set; } = 0.05m;

    // ─── SL/TP (ATR-based fallback when Run-level not set) ───
    public decimal AtrStopMultiplier { get; set; } = 2.0m;
    public decimal AtrTakeProfitMultiplier { get; set; } = 4.0m;

    // ─── Intra-Candle ───
    public bool EnableIntraCandleDecision { get; set; } = true;
    public TimeFrame SubCandleTimeFrame { get; set; } = TimeFrame.M5;

    /// <summary>
    /// تعداد کندل sub برای baseline. برای M5 در H1، 2000 کندل ≈ ۲۸ روز معاملاتی.
    /// برای Opening (۳۰ روز) + IntraDay (۱۰۰ کندل) با buffer کافی.
    /// </summary>
    public int SubCandleHistoryDepth { get; set; } = 2000;

    public decimal IntraCandleVolumeThreshold { get; set; } = 2.0m;
}