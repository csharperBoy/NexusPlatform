namespace Trader.Application.Dtos.HistoricalData;

public sealed record CreateHistoricalRecurringModel(
    string Name,
    IReadOnlyList<string> Isins,
    IReadOnlyList<int> TimeFrames,
    DateTime DateFrom,
    string RecurrencePatternJson);