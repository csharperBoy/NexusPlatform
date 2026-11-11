namespace Trader.Application.Dtos.HistoricalData;

public sealed record CreateHistoricalBackfillModel(
    string Name,
    IReadOnlyList<string> Isins,
    IReadOnlyList<int> TimeFrames,
    DateTime DateFrom,
    DateTime DateTo);