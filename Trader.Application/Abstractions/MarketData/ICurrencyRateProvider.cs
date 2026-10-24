namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// نرخ برابری ارز در یک لحظه‌ی مشخص.
/// Phase 1: هیچ Implementation وجود ندارد — فقط Interface.
/// Phase 3: Adapter به Nima/Sana/Sarafi آنلاین.
/// </summary>
public interface ICurrencyRateProvider
{
    /// <summary>
    /// نرخ تبدیل از fromCurrency به toCurrency در atTime.
    /// مثال: GetRateAsync("USD", "IRR", candle.OpenTime) → 650,000
    /// </summary>
    Task<decimal> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateTime atTime,
        CancellationToken ct = default);
}