using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// تبدیل Candleها به یک نمایش دیگر (مثلاً از IRR به USD).
/// Transformation فقط روی OHLC اعمال می‌شود؛ Volume (تعداد سهم) دست‌نخورده.
/// </summary>
public interface ICandleTransformer
{
    /// <summary>
    /// Candleهای IRR را به targetCurrency تبدیل می‌کند.
    /// نرخ ارز در atTime استفاده می‌شود (آخرین نرخ قبل از atTime).
    /// اگر نرخ موجود نباشد، CurrencyRateNotFoundException.
    /// </summary>
    Task<IReadOnlyList<Candle>> TransformAsync(
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default);
}