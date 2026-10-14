namespace Trader.Application.Dtos.Signals;

public sealed record SignalView(
    Guid Id,
    Guid RunId,
    string Isin,
    int Action,
    string ActionName,
    decimal Strength,
    string StrategyKey,
    DateTime ProducedAt,
    DateTime? ConsumedAt);