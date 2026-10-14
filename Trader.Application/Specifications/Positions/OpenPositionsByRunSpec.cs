using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.Positions;

public sealed class OpenPositionsByRunSpec : BaseSpecification<Position>
{
    public OpenPositionsByRunSpec(Guid runId)
        : base(p => p.RunId == runId && p.State != PositionState.Closed)
    {
    }
}