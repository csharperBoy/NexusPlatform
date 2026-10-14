using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.Events;

public sealed record SignalGeneratedEvent(
    Guid SignalId,
    Guid RunId,
    string Isin,
    SignalAction Action,
    decimal Strength,
    DateTime ProducedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}