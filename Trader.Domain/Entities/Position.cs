using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// موقعیت باز (یا بسته) یک Run روی یک Instrument در یک Account.
/// پس از هر Fill به‌روز می‌شود.
/// </summary>
public class Position : BaseEntity, IAuditableEntity
{
    public Guid RunId { get; private set; }
    public Guid TraderAccountId { get; private set; }
    public string Isin { get; private set; } = default!;

    public PositionState State { get; private set; }
    public Quantity Quantity { get; private set; }
    public Price AverageEntryPrice { get; private set; }

    public Money RealizedPnL { get; private set; }
    public Money? UnrealizedPnL { get; private set; }

    public Price? StopLossPrice { get; private set; }
    public Price? TakeProfitPrice { get; private set; }

    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private Position() { } // EF

    public static Position Open(
        Guid runId,
        Guid traderAccountId,
        string isin,
        Quantity quantity,
        Price entryPrice,
        Price? stopLossPrice,
        Price? takeProfitPrice)
    {
        if (quantity.IsZero)
            throw new InvalidOperationException("Cannot open position with zero quantity.");

        return new Position
        {
            RunId = runId,
            TraderAccountId = traderAccountId,
            Isin = isin,
            State = PositionState.Open,
            Quantity = quantity,
            AverageEntryPrice = entryPrice,
            RealizedPnL = Money.Zero(),
            StopLossPrice = stopLossPrice,
            TakeProfitPrice = takeProfitPrice,
            OpenedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ApplyBuyFill(Quantity qty, Price price)
    {
        var newTotalQty = Quantity.Value + qty.Value;
        var newAvg = ((AverageEntryPrice.Value * Quantity.Value) + (price.Value * qty.Value))
                     / newTotalQty;

        Quantity = new Quantity(newTotalQty);
        AverageEntryPrice = new Price(newAvg);
        Touch();
    }

    public void ApplySellFill(Quantity qty, Price price)
    {
        if (qty.Value > Quantity.Value)
            throw new InvalidOperationException("Sell quantity exceeds position quantity.");

        var realized = (price.Value - AverageEntryPrice.Value) * qty.Value;
        RealizedPnL = RealizedPnL.Add(new Money(realized));

        var remaining = Quantity.Value - qty.Value;
        Quantity = new Quantity(remaining);

        if (remaining == 0)
        {
            State = PositionState.Closed;
            ClosedAt = DateTime.UtcNow;
        }
        else
        {
            State = PositionState.PartiallyClosed;
        }

        Touch();
    }

    public void UpdateUnrealizedPnL(Price currentPrice)
    {
        var pnl = (currentPrice.Value - AverageEntryPrice.Value) * Quantity.Value;
        UnrealizedPnL = new Money(pnl);
        Touch();
    }

    public bool ShouldTriggerStopLoss(Price currentPrice)
        => State != PositionState.Closed
           && StopLossPrice.HasValue
           && currentPrice.Value <= StopLossPrice.Value.Value;

    public bool ShouldTriggerTakeProfit(Price currentPrice)
        => State != PositionState.Closed
           && TakeProfitPrice.HasValue
           && currentPrice.Value >= TakeProfitPrice.Value.Value;
}