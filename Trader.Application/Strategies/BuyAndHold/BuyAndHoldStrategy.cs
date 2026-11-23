using Trader.Application.Abstractions.Strategies;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Strategies.BuyAndHold;

public sealed class BuyAndHoldStrategy : IStrategy
{
    private readonly BuyAndHoldStrategyConfig _config;

    public BuyAndHoldStrategy(BuyAndHoldStrategyConfig config) => _config = config;

    public string Key => BuyAndHoldStrategyRegistration.KeyName;
    public string DisplayName => BuyAndHoldStrategyRegistration.DisplayNameValue;

    public int RequiredCandleCount => 1;
    public bool RequiresClosedCandlesOnly => false;

    public IReadOnlyList<CandleRequirement> ExtraCandleRequirements
        => Array.Empty<CandleRequirement>();

    public StrategyEvaluationResult Evaluate(StrategyInput input)
    {
        // Buy exactly once — on the very first valid evaluation
        if (input.Candles.Count != RequiredCandleCount)
            return StrategyEvaluationResult.Empty();

        var sig = new StrategySignal(
            Isin: input.Isin,
            Action: SignalAction.Buy,
            Strength: new SignalStrength(1.0m),
            MetadataJson: null,
            StopLoss: null,
            TakeProfit: null);

        return new StrategyEvaluationResult(new[] { sig }, Features: null);
    }
}