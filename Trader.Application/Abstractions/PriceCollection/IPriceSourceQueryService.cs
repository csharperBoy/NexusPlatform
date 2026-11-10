// IPriceSourceQueryService.cs
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Abstractions.PriceCollection;

public interface IPriceSourceQueryService
{
    Task<IReadOnlyList<PriceSourceView>> GetListAsync(CancellationToken ct = default);
    Task<PriceSourceView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}