using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.Events;

public sealed record PositionChangedEvent(
    Guid PositionId,
    Guid RunId,
    Guid TraderAccountId,
    string Isin,
    PositionState State,
    long Quantity,
    decimal AverageEntryPrice,
    DateTime ChangedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}