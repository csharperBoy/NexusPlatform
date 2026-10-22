using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// ذخیره‌ی کندل‌های بسته‌شده در DB. Idempotent بر اساس (Isin, TF, OpenTime).
/// </summary>
public interface ICandlePersistenceService
{
    /// <summary>
    /// ذخیره‌ی کندل‌های جدید (که هنوز در DB نیستند).
    /// خروجی: تعداد کندل‌های جدید ذخیره‌شده.
    /// </summary>
    Task<int> PersistAsync(
        string isin,
        int timeFrame,
        IReadOnlyList<Candle> candles,
        string source,
        CancellationToken ct = default);
}