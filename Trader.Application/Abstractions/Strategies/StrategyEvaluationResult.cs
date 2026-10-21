using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// خروجی ارزیابی یک Strategy.
/// لیست Signals به دلیل آماده‌بودن برای Ensemble آینده.
/// </summary>
public sealed record StrategyEvaluationResult(
    IReadOnlyList<StrategySignal> Signals,
    IReadOnlyDictionary<string, object?>? Features = null)
{
    /// <summary>بدون Signal — یا با Features (Hold) یا null (Warmup).</summary>
    public static StrategyEvaluationResult Empty(
        IReadOnlyDictionary<string, object?>? features = null)
        => new(Array.Empty<StrategySignal>(), features);
}

public sealed record StrategySignal(
    string Isin,
    SignalAction Action,
    SignalStrength Strength,
    string? MetadataJson = null);