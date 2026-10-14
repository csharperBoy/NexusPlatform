using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.BrokerOrders;

public sealed class BrokerOrderByExternalIdSpec : BaseSpecification<BrokerOrder>
{
    public BrokerOrderByExternalIdSpec(Guid accountId, string externalOrderId)
        : base(o => o.TraderAccountId == accountId
                 && o.BrokerOrderId == externalOrderId)
    {
    }
}