using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.Events;

public sealed record OrderIntentCreatedEvent(
    Guid OrderIntentId,
    Guid RunId,
    Guid TraderAccountId,
    string Isin,
    OrderSide Side,
    decimal Price,
    long Quantity,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}