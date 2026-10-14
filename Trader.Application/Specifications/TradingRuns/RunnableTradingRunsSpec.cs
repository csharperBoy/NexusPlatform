using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.TradingRuns;

public sealed class RunnableTradingRunsSpec : BaseSpecification<TradingRun>
{
    public RunnableTradingRunsSpec()
        : base(r => r.State == TradingRunState.Ready
                 || r.State == TradingRunState.Running)
    {
        AddInclude(r => r.Symbols);
        AddInclude(r => r.Accounts);
    }
}