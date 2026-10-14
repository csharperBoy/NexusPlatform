using Core.Domain.Common;

namespace Trader.Domain.Events;

public sealed record TradingRunStartedEvent(
    Guid RunId,
    DateTime StartedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}