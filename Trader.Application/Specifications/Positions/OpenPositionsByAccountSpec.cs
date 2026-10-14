using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.Positions;

public sealed class OpenPositionsByAccountSpec : BaseSpecification<Position>
{
    public OpenPositionsByAccountSpec(Guid accountId)
        : base(p => p.TraderAccountId == accountId
                 && p.State != PositionState.Closed)
    {
    }
}