using Core.Domain.Common;

namespace Trader.Domain.Events;

public sealed record TradingRunFaultedEvent(
    Guid RunId,
    string Reason,
    DateTime FaultedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}