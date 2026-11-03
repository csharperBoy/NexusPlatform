namespace Trader.Application.Dtos.Backtests;

public sealed record BacktestEventQuery(
    Guid BacktestRunId,
    int? Type = null,
    int? MinSeverity = null,
    string? Isin = null,
    string? Search = null,
    int Skip = 0,
    int Take = 500);