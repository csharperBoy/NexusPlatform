using Trader.Application.Dtos.Orders;

namespace Trader.Application.Abstractions.Orders;

public interface IBrokerOrderQueryService
{
    Task<(IReadOnlyList<BrokerOrderView> Items, int TotalCount)> QueryAsync(
        BrokerOrderQuery query, CancellationToken ct = default);

    Task<BrokerOrderDetailView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}