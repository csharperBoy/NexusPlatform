using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.OrderIntents;

/// <summary>
/// Sell OrderIntentهای باز برای یک (Run, Account, Isin).
/// برای جلوگیری از ارسال Sell تکراری هنگام SL/TP Trigger.
/// </summary>
public sealed class OpenSellIntentsByPositionSpec : BaseSpecification<OrderIntent>
{
    public OpenSellIntentsByPositionSpec(Guid runId, Guid accountId, string isin)
        : base(i => i.RunId == runId
                 && i.TraderAccountId == accountId
                 && i.Isin == isin
                 && i.Side == OrderSide.Sell
                 && (i.State == OrderIntentState.Pending
                  || i.State == OrderIntentState.Dispatched
                  || i.State == OrderIntentState.Acknowledged))
    {
    }
}