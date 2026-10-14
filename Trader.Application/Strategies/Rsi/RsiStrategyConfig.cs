using Trader.Application.Abstractions.Strategies;

namespace Trader.Application.Strategies.Rsi;

public sealed class RsiStrategyConfig : StrategyConfig
{
    public override string StrategyKey => RsiStrategyRegistration.KeyName;

    public int Period { get; set; } = 14;
    public decimal OversoldThreshold { get; set; } = 30m;
    public decimal OverboughtThreshold { get; set; } = 70m;
    public decimal MinSignalStrength { get; set; } = 0.6m;
}