namespace Trader.Application.Dtos.Backtests;

public sealed record BacktestSymbolModel(
    string Isin,
    decimal AllocationPercentage);