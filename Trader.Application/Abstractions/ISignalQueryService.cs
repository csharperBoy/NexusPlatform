using Trader.Application.Dtos.Signals;

namespace Trader.Application.Abstractions;

public interface ISignalQueryService
{
    Task<IReadOnlyList<SignalView>> GetRecentByRunAsync(
        Guid runId, int take = 100, CancellationToken ct = default);
}