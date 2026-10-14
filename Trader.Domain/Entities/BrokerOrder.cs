using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// نتیجه‌ی واقعی سفارش در کارگزاری. یک‌به‌یک با OrderIntent (در حالت عادی).
/// </summary>
public class BrokerOrder : BaseEntity, IAuditableEntity
{
    public Guid? OrderIntentId { get; private set; }
    public Guid RunId { get; private set; }
    public Guid TraderAccountId { get; private set; }
    public string? BrokerOrderId { get; private set; }

    public string Isin { get; private set; } = default!;
    public OrderSide Side { get; private set; }
    public Price Price { get; private set; }
    public Quantity Quantity { get; private set; }
    public Quantity FilledQuantity { get; private set; }

    public OrderStateKind State { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private BrokerOrder() { } // EF

    public static BrokerOrder CreatePending(
        Guid runId,
        Guid traderAccountId,
        Guid? orderIntentId,
        string isin,
        OrderSide side,
        Price price,
        Quantity quantity)
    {
        return new BrokerOrder
        {
            RunId = runId,
            TraderAccountId = traderAccountId,
            OrderIntentId = orderIntentId,
            Isin = isin,
            Side = side,
            Price = price,
            Quantity = quantity,
            FilledQuantity = Quantity.Zero,
            State = OrderStateKind.Pending,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignBrokerOrderId(string brokerOrderId)
    {
        BrokerOrderId = brokerOrderId;
        Touch();
    }

    public void ApplyStateUpdate(OrderStateKind newState, Quantity filledQuantity)
    {
        State = newState;
        FilledQuantity = filledQuantity;
        Touch();
    }

    public decimal FilledRatio =>
        Quantity.IsZero ? 0m : (decimal)FilledQuantity.Value / Quantity.Value;
}