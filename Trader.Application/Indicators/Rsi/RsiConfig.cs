namespace Trader.Application.Indicators.Rsi;

/// <summary>
/// تنظیمات Indicator RSI — مستقل از Strategy.
/// (Thresholdها و MinSignalStrength در RsiStrategyConfig هستند.)
/// </summary>
public sealed class RsiConfig
{
    public int Period { get; set; } = 14;
}