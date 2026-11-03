namespace Trader.Application.Dtos.Backtests;

public sealed record BacktestEventView(
    Guid Id,
    Guid BacktestRunId,
    int Type,
    string TypeName,
    int Severity,
    string SeverityName,
    DateTime OccurredAt,
    string Message,
    string? PayloadJson,
    string? Isin,
    DateTime? CandleTime,
    Guid? TradeId);