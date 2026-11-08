using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Risk;

public sealed class RunAccountDailyEquitySnapshotByRunAccountDateSpec
    : BaseSpecification<RunAccountDailyEquitySnapshot>
{
    public RunAccountDailyEquitySnapshotByRunAccountDateSpec(
        Guid runId, Guid accountId, DateOnly tradingDate)
        : base(x => x.RunId == runId
                 && x.AccountId == accountId
                 && x.TradingDate == tradingDate)
    {
    }
}