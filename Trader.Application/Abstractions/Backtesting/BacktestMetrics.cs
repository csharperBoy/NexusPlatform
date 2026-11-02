namespace Trader.Application.Abstractions.Backtesting;

public sealed record BacktestMetrics(
    decimal TotalReturnPct,
    decimal MaxDrawdownPct,
    decimal SharpeRatio,
    decimal WinRate,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    decimal ProfitFactor,
    decimal AvgWin,
    decimal AvgLoss);