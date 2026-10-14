using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Abstractions;

public interface ITradingRunQueryService
{
    Task<IReadOnlyList<TradingRunInfoView>> GetListAsync(CancellationToken ct = default);
    Task<TradingRunDetailView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}