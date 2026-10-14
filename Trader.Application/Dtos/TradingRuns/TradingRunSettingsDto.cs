namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunSettingsDto(
    int CapitalAllocationMode,
    decimal? FixedCapitalAmount,
    decimal? StopLossPercentage,
    decimal? StopLossPrice,
    decimal? TakeProfitPercentage,
    decimal? TakeProfitPrice,
    int DefaultOrderMode,
    int OnMarketClose,
    int OnStop);