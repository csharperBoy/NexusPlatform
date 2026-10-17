using Core.Domain.Common.EntityProperties;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// یک Fill از یک سفارش. Idempotent بر اساس TradeNumber.
/// </summary>
public class OrderTrade : BaseEntity
{
    public Guid BrokerOrderId { get; private set; }

    /// <summary>شماره یکتای معامله در بورس.</summary>
    public long TradeNumber { get; private set; }

    public string Isin { get; private set; } = default!;
    public Trader.Domain.Enums.OrderSide Side { get; private set; }
    public Price Price { get; private set; }
    public Quantity Quantity { get; private set; }

    /// <summary>زمان واقعی معامله در بازار.</summary>
    public DateTimeOffset TradeDate { get; private set; }

    /// <summary>آیا بعداً کنسل شده است.</summary>
    public bool IsCanceled { get; private set; }
    public DateTimeOffset? CancelDateTime { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private OrderTrade() { } // EF

    public static OrderTrade Create(
        Guid brokerOrderId,
        long tradeNumber,
        string isin,
        Trader.Domain.Enums.OrderSide side,
        Price price,
        Quantity quantity,
        DateTimeOffset tradeDate)
    {
        return new OrderTrade
        {
            BrokerOrderId = brokerOrderId,
            TradeNumber = tradeNumber,
            Isin = isin,
            Side = side,
            Price = price,
            Quantity = quantity,
            TradeDate = tradeDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkCanceled(DateTimeOffset cancelDateTime)
    {
        IsCanceled = true;
        CancelDateTime = cancelDateTime;
    }
}