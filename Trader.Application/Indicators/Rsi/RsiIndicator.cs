using Trader.Application.Abstractions.Indicators;

namespace Trader.Application.Indicators.Rsi;

/// <summary>
/// Adapter روی RsiCalculator — IIndicator را پیاده می‌کند.
/// منطق محاسبه در RsiCalculator باقی می‌ماند (تست‌پذیر، Pure).
/// </summary>
public sealed class RsiIndicator : IIndicator<RsiConfig, decimal?>
{
    public string Key => "rsi";
    public string DisplayName => "RSI";

    public decimal? Compute(IndicatorInput input, RsiConfig config)
    {
        if (config.Period <= 0)
            return null;

        if (input.Candles.Count < config.Period + 1)
            return null;

        var closes = input.Candles.Select(c => c.Close).ToList();
        return RsiCalculator.Calculate(closes, config.Period);
    }
}