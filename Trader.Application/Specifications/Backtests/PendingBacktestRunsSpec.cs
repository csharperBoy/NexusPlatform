using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.Backtests;

public sealed class PendingBacktestRunsSpec : BaseSpecification<BacktestRun>
{
    public PendingBacktestRunsSpec()
        : base(b => b.Status == BacktestStatus.Pending)
    {
        ApplyOrderBy(b => b.CreatedAt);
    }
}