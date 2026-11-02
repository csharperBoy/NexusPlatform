using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Abstractions.Backtesting;

public interface IBacktestRunQueryService
{
    Task<IReadOnlyList<BacktestRunSummaryView>> GetListAsync(
        Guid? sourceRunId,
        int take = 100,
        CancellationToken ct = default);

    Task<BacktestRunDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<BacktestTradeView>> GetTradesAsync(
        Guid backtestRunId, CancellationToken ct = default);

    Task<IReadOnlyList<BacktestDailyEquityView>> GetEquityCurveAsync(
        Guid backtestRunId, CancellationToken ct = default);

    Task<IReadOnlyList<BacktestRunSummaryView>> CompareAsync(
        List<Guid> ids, CancellationToken ct = default);
}