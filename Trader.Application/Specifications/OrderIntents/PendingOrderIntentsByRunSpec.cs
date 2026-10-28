using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.OrderIntents;

public sealed class PendingOrderIntentsByRunSpec : BaseSpecification<OrderIntent>
{
    public PendingOrderIntentsByRunSpec(Guid runId)
        : base(i => i.RunId == runId
                 && (i.State == OrderIntentState.Pending
                  || i.State == OrderIntentState.Dispatched
                  || i.State == OrderIntentState.Acknowledged))
    {
    }
}