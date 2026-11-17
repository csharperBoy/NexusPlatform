using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.Tuning;

public sealed class PendingStrategyTuningRunsSpec : BaseSpecification<StrategyTuningRun>
{
    public PendingStrategyTuningRunsSpec()
        : base(r => r.Status == TuningStatus.Pending)
    {
        ApplyOrderBy(r => r.CreatedAt);
    }
}