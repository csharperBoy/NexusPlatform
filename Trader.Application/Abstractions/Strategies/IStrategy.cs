namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// Strategy خالص — Sync، بدون I/O، بدون دسترسی به Account/Broker.
/// </summary>
public interface IStrategy
{
    string Key { get; }
    string DisplayName { get; }

    StrategyEvaluationResult Evaluate(StrategyInput input);
}