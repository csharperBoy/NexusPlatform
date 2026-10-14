namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunDetailView(
    Guid Id,
    string Name,
    int State,
    string StateName,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    TradingRunSettingsDto Settings,
    IReadOnlyList<TradingRunSymbolDto> Symbols,
    IReadOnlyList<Guid> AccountIds,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime CreatedAt,
    DateTime? ModifiedAt);