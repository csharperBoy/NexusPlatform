using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Abstractions.HistoricalData;

public interface IHistoricalDataCollectionQueryService
{
    Task<IReadOnlyList<HistoricalDataCollectionJobSummaryView>> GetListAsync(
        int take = 50, CancellationToken ct = default);

    Task<HistoricalDataCollectionJobDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default);
}