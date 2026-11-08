using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Risk;

public sealed class RunDailyEquitySnapshotByRunAndDateSpec
    : BaseSpecification<RunDailyEquitySnapshot>
{
    public RunDailyEquitySnapshotByRunAndDateSpec(Guid runId, DateOnly tradingDate)
        : base(x => x.RunId == runId && x.TradingDate == tradingDate)
    {
    }
}