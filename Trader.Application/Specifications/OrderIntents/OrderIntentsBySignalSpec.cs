using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.OrderIntents;

public sealed class OrderIntentsBySignalSpec : BaseSpecification<OrderIntent>
{
    public OrderIntentsBySignalSpec(Guid sourceSignalId)
        : base(i => i.SourceSignalId == sourceSignalId)
    {
    }
}