using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;

namespace Trader.Application.Strategies.BuyAndHold;

public sealed class BuyAndHoldStrategyRegistration
    : StrategyRegistration<BuyAndHoldStrategyConfig>
{

    public BuyAndHoldStrategyRegistration()
    {
    }

    public const string KeyName = "buyandhold";
    public const string DisplayNameValue = "Buy and Hold";

    public override string Key => KeyName;
    public override string DisplayName => DisplayNameValue;

    protected override IStrategy CreateStrategy(BuyAndHoldStrategyConfig config)
        => new BuyAndHoldStrategy(config);
}