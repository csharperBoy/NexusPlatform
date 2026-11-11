using Trader.Application.Dtos.Orders;

namespace Trader.Application.Abstractions.Orders;

public interface IOrderIntentQueryService
{
    Task<(IReadOnlyList<OrderIntentView> Items, int TotalCount)> QueryAsync(
        OrderIntentQuery query, CancellationToken ct = default);

    Task<OrderIntentView?> GetByIdAsync(Guid id, CancellationToken ct = default);
}