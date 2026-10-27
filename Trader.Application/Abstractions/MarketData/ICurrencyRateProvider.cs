namespace Trader.Application.Abstractions.MarketData;

public interface ICurrencyRateProvider
{
    /// <summary>
    /// آخرین نرخ برای یک Pair در لحظه‌ی atTime.
    /// </summary>
    Task<decimal> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateTime atTime,
        CancellationToken ct = default);

    /// <summary>
    /// همه‌ی نرخ‌های یک Pair تا لحظه‌ی toTime (صعودی بر اساس EffectiveAt).
    /// برای Per-Candle Transformation استفاده می‌شود.
    /// </summary>
    Task<IReadOnlyList<CurrencyRatePoint>> GetRateHistoryAsync(
        string fromCurrency,
        string toCurrency,
        DateTime toTime,
        CancellationToken ct = default);
}

/// <summary>یک نقطه‌ی نرخ در تاریخ. Rate = مقدار toCurrency برای ۱ fromCurrency.</summary>
public sealed record CurrencyRatePoint(
    decimal Rate,
    DateTime EffectiveAt,
    string Source);