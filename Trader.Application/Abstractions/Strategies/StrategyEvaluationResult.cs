using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// خروجی ارزیابی یک Strategy.
/// لیست Signals به دلیل آماده‌بودن برای Ensemble آینده.
/// </summary>
public sealed record StrategyEvaluationResult(
    IReadOnlyList<StrategySignal> Signals)
{
    public static StrategyEvaluationResult Empty
        => new(Array.Empty<StrategySignal>());
}

public sealed record StrategySignal(
    string Isin,
    SignalAction Action,
    SignalStrength Strength,
    string? MetadataJson = null);