namespace Trader.Application.Dtos.Backtests;

public sealed record BacktestTradeView(
    Guid Id,
    string Isin,
    DateTime EntryTime,
    DateTime LastEntryTime,
    long TotalEntryQuantity,
    decimal WeightedAvgEntryPrice,
    decimal TotalBuyCommission,
    DateTime ExitTime,
    decimal ExitPrice,
    long ExitQuantity,
    decimal TotalSellCommission,
    int ExitReason,
    string ExitReasonName,
    decimal GrossPnL,
    decimal NetPnL,
    decimal NetPnLPct,
    long HoldingDurationSeconds,
    string EntryFillsJson);