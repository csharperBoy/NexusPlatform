namespace Trader.Application.Dtos.TradingRuns;

public sealed record TradingRunSymbolDto(
    string Isin,
    decimal AllocationPercentage);