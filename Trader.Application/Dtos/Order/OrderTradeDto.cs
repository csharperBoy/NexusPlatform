namespace Trader.Application.Dtos.Order;

/// <summary>
/// یک fill event از یک سفارش.
/// هر سفارش می‌تونه چند تا از اینا داشته باشه (partial fill).
/// </summary>
public class OrderTradeDto
{
    // ── شناسه‌ها ──
    public string OrderId { get; set; } = "";          // isr — همون orderId از orderReport
    public long TradeNumber { get; set; }              // شماره‌ی معامله (یکتای بورس)
    public string InternalId { get; set; } = "";       // id داخلی DB کارگزاری
    public long RequestId { get; set; }
    public string CustomerIsin { get; set; } = "";

    // ── نماد ──
    public string SymbolIsin { get; set; } = "";
    public int Side { get; set; }                       // 0=buy, 1=sell

    // ── قیمت و حجم ──
    public long Price { get; set; }
    public long Quantity { get; set; }                  // حجم همین fill
    public long Remain { get; set; }                    // باقی‌مونده بعد از این fill
    public bool HasRemain { get; set; }

    // ── زمان ──
    /// <summary>زمان واقعی معامله در بازار (برای backtesting از این استفاده کن).</summary>
    public DateTimeOffset TradeDate { get; set; }

    /// <summary>زمان ثبت در سیستم کارگزاری.</summary>
    public DateTimeOffset CreateDateTime { get; set; }

    /// <summary>اگه این trade بعداً کنسل شده باشه.</summary>
    public DateTimeOffset? CancelDateTime { get; set; }
    public bool? IsCanceled { get; set; }

    // ── فیلدهای ناشناخته (ذخیره می‌شن، تفسیر نمی‌شن) ──
    public long Hon { get; set; }
    public int Partition { get; set; }
    public long ConsumerIndex { get; set; }
    public int Origin { get; set; }
}