using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.OrderTrades;

public sealed class TradesByBrokerOrderSpec : BaseSpecification<OrderTrade>
{
    public TradesByBrokerOrderSpec(Guid brokerOrderId)
        : base(t => t.BrokerOrderId == brokerOrderId)
    {
    }
}