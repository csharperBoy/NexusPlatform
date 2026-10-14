using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.Events;

public sealed record TradingRunStoppedEvent(
    Guid RunId,
    DateTime StoppedAt,
    StopRunBehavior Behavior) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}