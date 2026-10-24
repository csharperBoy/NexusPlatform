using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Rsi;

namespace Trader.Application.Strategies.Rsi;

public sealed class RsiStrategyRegistration
    : StrategyRegistration<RsiStrategyConfig>
{
    private readonly IIndicator<RsiConfig, decimal?> _rsiIndicator;

    public RsiStrategyRegistration(IIndicator<RsiConfig, decimal?> rsiIndicator)
    {
        _rsiIndicator = rsiIndicator;
    }

    public const string KeyName = "rsi";
    public const string DisplayNameValue = "RSI";

    public override string Key => KeyName;
    public override string DisplayName => DisplayNameValue;

    protected override IStrategy CreateStrategy(RsiStrategyConfig config)
        => new RsiStrategy(_rsiIndicator, config);
}