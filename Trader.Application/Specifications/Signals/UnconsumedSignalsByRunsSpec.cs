using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Signals;

public sealed class UnconsumedSignalsByRunsSpec : BaseSpecification<Signal>
{
    public UnconsumedSignalsByRunsSpec(IReadOnlyCollection<Guid> runIds)
        : base(s => s.ConsumedAt == null && runIds.Contains(s.RunId))
    {
        ApplyOrderBy(s => s.ProducedAt);
    }
}