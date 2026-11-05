using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.Positions;

/// <summary>
/// Positionهای باز non-paper — برای Reconciliation.
/// Positionهای Paper در DB هستند ولی Broker از آن‌ها خبر ندارد،
/// پس در Reconciliation نباید مقایسه شوند.
/// </summary>
public sealed class OpenPositionsWithoutPaperSpec : BaseSpecification<Position>
{
    public OpenPositionsWithoutPaperSpec()
        : base(p => !p.IsPaper
                 && p.State != PositionState.Closed
                 && p.Quantity.Value > 0)
    {
    }
}