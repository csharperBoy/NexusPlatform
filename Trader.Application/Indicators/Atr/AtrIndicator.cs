using Trader.Application.Abstractions.Indicators;

namespace Trader.Application.Indicators.Atr;

public sealed class AtrIndicator : IIndicator<AtrConfig, decimal?>
{
    public string Key => "atr";
    public string DisplayName => "ATR";

    public decimal? Compute(IndicatorInput input, AtrConfig config)
    {
        if (config.Period <= 0) return null;
        if (input.Candles.Count < config.Period + 1) return null;

        return AtrCalculator.Calculate(input.Candles, config.Period);
    }
}