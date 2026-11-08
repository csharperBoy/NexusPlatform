namespace Trader.Application.Dtos.TradingRuns;


public sealed record TradingRunAccountModel(
    Guid AccountId,
    decimal AllocationPercent = 100m);

public sealed record CreateTradingRunModel(
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    int ExecutionMode,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<TradingRunAccountModel> Accounts);   

public sealed record TradingRunSettingsModel(
    int CapitalAllocationMode,
    decimal? FixedCapitalAmount,
    decimal? StopLossPercentage,
    decimal? StopLossPrice,
    decimal? TakeProfitPercentage,
    decimal? TakeProfitPrice,
    int DefaultOrderMode,
    int OnMarketClose,
    int OnStop);

public sealed record TradingRunSymbolModel(
    string Isin,
    decimal AllocationPercentage);