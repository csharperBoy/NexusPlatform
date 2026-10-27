using Trader.Application.Dtos.TradingEvents;

namespace Trader.Application.Abstractions;

public interface ITradingEventQueryService
{
    Task<(IReadOnlyList<TradingEventView> Items, int TotalCount)> QueryAsync(
        TradingEventQuery query,
        CancellationToken ct = default);

    Task<TradingEventView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}