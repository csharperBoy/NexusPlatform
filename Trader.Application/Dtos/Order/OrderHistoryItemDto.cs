using Trader.Domain.Enums;

namespace Trader.Application.Dtos.Order;

public class OrderHistoryItemDto
{
    // ── شناسه‌ها ──
    public string OrderId { get; set; } = "";          // شناسه‌ی کارگزاری
    public string InternalId { get; set; } = "";        // id داخلی DB کارگزاری
    public string ReferenceId { get; set; } = "";
    public string ParentId { get; set; } = "";
    public string CorrelationId { get; set; } = "";     // برای ردیابی سفارشات خودمون
    public string CustomerIsin { get; set; } = "";

    // ── نماد ──
    public string SymbolIsin { get; set; } = "";
    public string SymbolName { get; set; } = "";

    // ── قیمت و حجم ──
    public long Price { get; set; }
    public long MeanPrice { get; set; }        // میانگین قیمت اجرا
    public long Quantity { get; set; }
    public long ExecutedQuantity { get; set; }
    public long OrderValue { get; set; }

    // ── نوع و وضعیت ──
    public int Side { get; set; }               // 0=buy, 1=sell
    public int OrderStateRaw { get; set; }      // خام، از کارگزاری
    public OrderStateKind State { get; set; }   // تفسیرشده

    // ── زمان ──
    public DateTimeOffset CreateDateTime { get; set; }
    public DateTimeOffset? ModifyDateTime { get; set; }
    public DateTimeOffset? ValidityDate { get; set; }
    public int Validity { get; set; }

    // ── فلگ‌ها ──
    public bool IgnoreCheckingMoney { get; set; }
    public bool TraderCredit { get; set; }
    public string? Error { get; set; }

    // ── رخدادها ──
    public List<OrderSourceDto> Sources { get; set; } = new();

    // ── محاسبه‌شده ──
    public long RemainingQuantity => Quantity - ExecutedQuantity;
    public bool IsFullyExecuted => ExecutedQuantity == Quantity && Quantity > 0;
    public bool IsActive => State is OrderStateKind.Open
                                  or OrderStateKind.Pending
                                  or OrderStateKind.PartiallyExecuted;
}

public class OrderSourceDto
{
    public int ActionType { get; set; }         // 0=create, 2=modify/cancel
    public DateTimeOffset CreateDateTime { get; set; }
    public string Ip { get; set; } = "";        // "system" یا IP
    public int OrderFrom { get; set; }          // 34=desktop, 33=mobile, 101=system
}