namespace Trader.Application.Dtos.HistoricalData;

public sealed record HistoricalDataCollectionJobDetailView(
    Guid Id,
    string Name,
    int Status,
    string StatusName,
    List<string> Isins,
    List<int> TimeFrames,
    DateTime DateFrom,
    DateTime DateTo,
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