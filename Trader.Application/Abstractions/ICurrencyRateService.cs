using Trader.Application.Dtos.CurrencyRates;

namespace Trader.Application.Abstractions;

public interface ICurrencyRateService
{
    Task<Guid> CreateAsync(CreateCurrencyRateModel model);
    Task<decimal?> GetLatestAsync(
        string fromCurrency, string toCurrency, DateTime atTime);
    Task<IReadOnlyList<CurrencyRateView>> GetRecentAsync(
        string? fromCurrency, string? toCurrency, int limit = 100);
}