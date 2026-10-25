namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunInfoView(
    Guid Id,
    string Name,
    int State,
    string StateName,
    string StrategyKey,
    int PrimaryTimeFrame,
    string PrimaryTimeFrameName,
    int CapitalAllocationMode,
    decimal? FixedCapital,
    string? TargetCurrency,
    int SymbolCount,
    int AccountCount,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime CreatedAt);