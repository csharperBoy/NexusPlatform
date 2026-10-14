using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.BrokerOrders;

public sealed class OpenBrokerOrdersByAccountSpec : BaseSpecification<BrokerOrder>
{
    public OpenBrokerOrdersByAccountSpec(Guid accountId)
        : base(o => o.TraderAccountId == accountId)
    {
        // فیلتر State در Service Layer اعمال می‌شود، چون OrderStateKind در پلتفرم شما
        // مقادیر دقیقش متفاوت است. اینجا فقط Account فیلتر می‌کنیم.
        ApplyOrderByDescending(o => o.SubmittedAt);
    }
}