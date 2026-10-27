using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.MarketData;

public interface ICandleTransformer
{
    /// <summary>
    /// تبدیل Candleهای IRR به targetCurrency با Per-Candle Historical Rate.
    /// هر کندل با آخرین نرخ موجود در زمان OpenTime خودش تبدیل می‌شود.
    /// </summary>
    Task<IReadOnlyList<Candle>> TransformAsync(
        string isin,
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default);
}