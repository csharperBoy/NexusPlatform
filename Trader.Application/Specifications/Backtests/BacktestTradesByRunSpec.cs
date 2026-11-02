using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Backtests;

public sealed class BacktestTradesByRunSpec : BaseSpecification<BacktestTrade>
{
    public BacktestTradesByRunSpec(Guid backtestRunId)
        : base(t => t.BacktestRunId == backtestRunId)
    {
        ApplyOrderBy(t => t.EntryTime);
    }
}