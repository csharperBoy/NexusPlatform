using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// یک نیازمندی Sub-Candle از طرف Strategy.
/// Strategy می‌تواند چند TimeFrame همزمان بخواهد.
/// </summary>
public sealed record CandleRequirement(
    TimeFrame TimeFrame,
    int Count,
    bool ClosedOnly = true);