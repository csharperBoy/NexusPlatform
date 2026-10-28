using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// تصمیم Execution برای یک Signal در یک Account مشخص.
/// نتیجه‌ی واقعی سفارش در BrokerOrder نگه داشته می‌شود.
/// </summary>
public class OrderIntent : BaseEntity, IAuditableEntity
{
    public Guid RunId { get; private set; }
    public Guid TraderAccountId { get; private set; }
    public Guid? SourceSignalId { get; private set; }

    public string Isin { get; private set; } = default!;
    public OrderSide Side { get; private set; }
    public Price Price { get; private set; }
    public Quantity Quantity { get; private set; }
    public OrderMode OrderMode { get; private set; }

    public OrderIntentState State { get; private set; }
    public string? Reason { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTime? DispatchedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Price? StopLossPrice { get; private set; }
    public Price? TakeProfitPrice { get; private set; }
    public Guid? SourceEvaluationId { get; private set; }
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private OrderIntent() { } // EF

    public static OrderIntent Create(
    Guid runId,
    Guid traderAccountId,
    Guid? sourceSignalId,
    string isin,
    OrderSide side,
    Price price,
    Quantity quantity,
    OrderMode orderMode,
    string? reason = null,
    Price? stopLossPrice = null,
    Price? takeProfitPrice = null,
    Guid? sourceEvaluationId = null)
    {
        // ─── Validation ───
        if (string.IsNullOrWhiteSpace(isin))
            throw new ArgumentException(
                "Isin is required and cannot be null/empty.", nameof(isin));

        if (quantity.IsZero)
            throw new InvalidOperationException("Order quantity cannot be zero.");

        return new OrderIntent
        {
            RunId = runId,
            TraderAccountId = traderAccountId,
            SourceSignalId = sourceSignalId,
            Isin = isin,                        // ← چک کن این خط هست!
            Side = side,
            Price = price,
            Quantity = quantity,
            OrderMode = orderMode,
            State = OrderIntentState.Pending,
            Reason = reason,
            StopLossPrice = stopLossPrice,
            TakeProfitPrice = takeProfitPrice,
            SourceEvaluationId = sourceEvaluationId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkDispatched()
    {
        State = OrderIntentState.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkAcknowledged()
    {
        State = OrderIntentState.Acknowledged;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkRejected(string reason)
    {
        State = OrderIntentState.Rejected;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        State = OrderIntentState.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }
}