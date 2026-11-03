using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Abstractions.Backtesting;

public interface IBacktestEventQueryService
{
    Task<(IReadOnlyList<BacktestEventView> Items, int TotalCount)> QueryAsync(
        BacktestEventQuery query,
        CancellationToken ct = default);
}