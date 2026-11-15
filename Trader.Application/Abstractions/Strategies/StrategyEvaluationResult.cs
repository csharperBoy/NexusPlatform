using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Abstractions.Strategies;

public sealed record StrategyEvaluationResult(
    IReadOnlyList<StrategySignal> Signals,
    IReadOnlyDictionary<string, object?>? Features = null)
{
    public static StrategyEvaluationResult Empty(
        IReadOnlyDictionary<string, object?>? features = null)
        => new(Array.Empty<StrategySignal>(), features);
}

public sealed record StrategySignal(
    string Isin,
    SignalAction Action,
    SignalStrength Strength,
    string? MetadataJson = null,

    /// <summary>
    /// پیشنهاد Strategy برای Stop Loss. Run-level SL اگر set بود Override می‌کنه.
    /// فقط برای Buy معنی داره (Sell، Position رو می‌بنده).
    /// </summary>
    Price? StopLoss = null,

    /// <summary>پیشنهاد Strategy برای Take Profit. Run-level TP اگر set بود Override می‌کنه.</summary>
    Price? TakeProfit = null);