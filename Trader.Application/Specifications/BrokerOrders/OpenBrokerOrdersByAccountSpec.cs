using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.BrokerOrders;

public sealed class OpenBrokerOrdersByAccountSpec : BaseSpecification<BrokerOrder>
{
    public OpenBrokerOrdersByAccountSpec(Guid accountId)
        : base(o => o.TraderAccountId == accountId
                 && (o.State == OrderStateKind.Pending
                  || o.State == OrderStateKind.Open
                  || o.State == OrderStateKind.PartiallyExecuted))
    {
        ApplyOrderByDescending(o => o.SubmittedAt);
    }
}