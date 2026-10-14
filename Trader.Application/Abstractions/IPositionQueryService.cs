using Trader.Application.Dtos.Positions;

namespace Trader.Application.Abstractions;

public interface IPositionQueryService
{
    Task<IReadOnlyList<PositionView>> GetOpenByRunAsync(
        Guid runId, CancellationToken ct = default);

    Task<IReadOnlyList<PositionView>> GetOpenByAccountAsync(
        Guid accountId, CancellationToken ct = default);
}