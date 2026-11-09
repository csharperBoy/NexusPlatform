namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunInfoView(
    Guid Id,
    string Name,
    int State,
    string StateName,
    string StrategyKey,
    int PrimaryTimeFrame,
    string PrimaryTimeFrameName,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    int ExecutionMode,
    string ExecutionModeName,
    int SymbolCount,
    int AccountCount,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime CreatedAt);