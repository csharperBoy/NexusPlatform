namespace Trader.Application.Dtos.TradingRuns;


public sealed record CreateTradingRunModel(
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<Guid> AccountIds);

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