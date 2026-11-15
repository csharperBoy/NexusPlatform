namespace Trader.Application.Indicators.Macd;

public sealed class MacdConfig
{
    public int FastPeriod { get; set; } = 12;
    public int SlowPeriod { get; set; } = 26;
    public int SignalPeriod { get; set; } = 9;
}

/// <summary>خروجی MACD در یک لحظه. همه‌ی مقادیر pure EMA-difference هستند.</summary>
public sealed record MacdResult(
    decimal MacdLine,
    decimal SignalLine,
    decimal Histogram);