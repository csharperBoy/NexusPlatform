using Core.Domain.Common;

namespace Trader.Domain.Events;

public sealed record BrokerOrderPlacedEvent(
    Guid BrokerOrderId,
    Guid RunId,
    Guid TraderAccountId,
    string? ExternalBrokerOrderId,
    string Isin,
    DateTime SubmittedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}