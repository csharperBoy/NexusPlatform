namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunDetailView(
    Guid Id,
    string Name,
    int State,
    string StateName,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    int ExecutionMode,                       
    string ExecutionModeName,
    TradingRunSettingsDto Settings,
    IReadOnlyList<TradingRunSymbolDto> Symbols,
    IReadOnlyList<TradingRunAccountDto> Accounts,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime CreatedAt,
    DateTime? ModifiedAt);


public sealed record TradingRunAccountDto(
    Guid AccountId,
    decimal AllocationPercent);