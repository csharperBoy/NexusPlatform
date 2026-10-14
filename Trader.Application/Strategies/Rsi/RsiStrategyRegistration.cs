using Trader.Application.Abstractions.Strategies;

namespace Trader.Application.Strategies.Rsi;

public sealed class RsiStrategyRegistration
    : StrategyRegistration<RsiStrategyConfig>
{
    public const string KeyName = "rsi";
    public const string DisplayNameValue = "RSI";

    public override string Key => KeyName;
    public override string DisplayName => DisplayNameValue;

    protected override IStrategy CreateStrategy(RsiStrategyConfig config)
        => new RsiStrategy(config);
}