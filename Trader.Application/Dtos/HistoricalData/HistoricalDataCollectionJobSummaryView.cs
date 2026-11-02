namespace Trader.Application.Dtos.HistoricalData;

public sealed record HistoricalDataCollectionJobSummaryView(
    Guid Id,
    string Name,
    int Status,
    string StatusName,
    int TotalCombinations,
    int CompletedCombinations,
    int TotalCandlesCollected,
    decimal ProgressPercent,
    string? CurrentIsin,
    int? CurrentTimeFrame,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    long? DurationMs,
    string? ErrorMessage,
    DateTime CreatedAt);