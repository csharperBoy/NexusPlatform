using Trader.Application.Abstractions.Strategies;

namespace Trader.Application.Strategies.BuyAndHold;

public sealed class BuyAndHoldStrategyConfig : StrategyConfig
{
    public override string StrategyKey => BuyAndHoldStrategyRegistration.KeyName;
}