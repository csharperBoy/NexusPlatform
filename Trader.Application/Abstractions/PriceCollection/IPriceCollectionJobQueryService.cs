// IPriceCollectionJobQueryService.cs
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Abstractions.PriceCollection;

public interface IPriceCollectionJobQueryService
{
    Task<IReadOnlyList<PriceCollectionJobView>> GetListAsync(
        Guid? priceSourceId, int take = 100, CancellationToken ct = default);
    Task<PriceCollectionJobView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}