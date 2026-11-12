using Trader.Application.Dtos.CurrencyRates;

namespace Trader.Application.Abstractions;

public interface ICurrencyRateService
{
    Task<Guid> CreateAsync(CreateCurrencyRateModel model);

    Task<decimal?> GetLatestAsync(
        string fromCurrency, string toCurrency, DateTime atTime);

    Task<IReadOnlyList<CurrencyRateView>> GetRecentAsync(
        string? fromCurrency, string? toCurrency, int limit = 100);

    /// <summary>
    /// آیا نرخی برای (from, to) — یا معکوسش — در بازه‌ی [fromDate, toDate] وجود دارد؟
    /// </summary>
    Task<bool> CheckRangeExistsAsync(
        string fromCurrency,
        string toCurrency,
        DateTime fromDate,
        DateTime toDate);
}