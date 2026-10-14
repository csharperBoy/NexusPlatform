using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Signals;

public sealed class RecentSignalsByRunSpec : BaseSpecification<Signal>
{
    public RecentSignalsByRunSpec(Guid runId, int take = 100)
        : base(s => s.RunId == runId)
    {
        ApplyOrderByDescending(s => s.ProducedAt);
        ApplyPaging(0, take);
    }
}