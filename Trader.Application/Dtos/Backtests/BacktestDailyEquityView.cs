namespace Trader.Application.Dtos.Backtests;

public sealed record BacktestDailyEquityView(
    DateTime Date,
    decimal Equity,
    decimal Cash,
    decimal PositionValue,
    decimal DailyReturnPct);