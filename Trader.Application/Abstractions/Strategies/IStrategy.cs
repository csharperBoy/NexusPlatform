namespace Trader.Application.Abstractions.Strategies;

public interface IStrategy
{
    string Key { get; }
    string DisplayName { get; }

    /// <summary>
    /// تعداد کندل مورد نیاز Strategy. اگر ۰ یا کمتر، Candles Fetch نمی‌شود.
    /// </summary>
    int RequiredCandleCount { get; }

    StrategyEvaluationResult Evaluate(StrategyInput input);
}