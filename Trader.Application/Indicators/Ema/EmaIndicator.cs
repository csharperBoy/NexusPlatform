using Trader.Application.Abstractions.Indicators;

namespace Trader.Application.Indicators.Ema;

public sealed class EmaIndicator : IIndicator<EmaConfig, decimal?>
{
    public string Key => "ema";
    public string DisplayName => "EMA";

    public decimal? Compute(IndicatorInput input, EmaConfig config)
    {
        if (config.Period <= 0) return null;
        if (input.Candles.Count < config.Period) return null;

        var closes = input.Candles.Select(c => c.Close).ToList();
        return EmaCalculator.CalculateLast(closes, config.Period);
    }
}