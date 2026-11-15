using Trader.Application.Abstractions.Indicators;

namespace Trader.Application.Indicators.Macd;

public sealed class MacdIndicator : IIndicator<MacdConfig, MacdResult?>
{
    public string Key => "macd";
    public string DisplayName => "MACD";

    public MacdResult? Compute(IndicatorInput input, MacdConfig config)
    {
        // MACD needs at least slowPeriod + signalPeriod candles for histogram
        var minRequired = config.SlowPeriod + config.SignalPeriod;
        if (input.Candles.Count < minRequired) return null;

        var closes = input.Candles.Select(c => c.Close).ToList();
        return MacdCalculator.CalculateLast(closes, config);
    }
}