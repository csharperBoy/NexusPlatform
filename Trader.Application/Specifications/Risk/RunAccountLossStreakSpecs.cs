using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Risk;

public sealed class RunAccountLossStreakByRunAccountSpec
    : BaseSpecification<RunAccountLossStreak>
{
    public RunAccountLossStreakByRunAccountSpec(Guid runId, Guid accountId)
        : base(x => x.RunId == runId && x.AccountId == accountId)
    {
    }
}

public sealed class RunAccountLossStreaksByRunSpec
    : BaseSpecification<RunAccountLossStreak>
{
    public RunAccountLossStreaksByRunSpec(Guid runId)
        : base(x => x.RunId == runId)
    {
    }
}

public sealed class RunAccountLossStreaksByAccountSpec
    : BaseSpecification<RunAccountLossStreak>
{
    public RunAccountLossStreaksByAccountSpec(Guid accountId)
        : base(x => x.AccountId == accountId)
    {
    }
}

public sealed class ActiveCircuitBreakersSpec
    : BaseSpecification<RunAccountLossStreak>
{
    public ActiveCircuitBreakersSpec()
        : base(x => x.CircuitBrokenAt != null)
    {
    }
}