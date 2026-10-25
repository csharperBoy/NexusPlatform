using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.CurrencyRates;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.MarketData;

namespace Trader.Infrastructure.Services;

public sealed class CurrencyRateService : ICurrencyRateService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly CurrencyRateCache _cache;
    private readonly ILogger<CurrencyRateService> _logger;

    public CurrencyRateService(
        ITraderUnitOfWork<TraderDbContext> uow,
        CurrencyRateCache cache,
        ILogger<CurrencyRateService> logger)
    {
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateCurrencyRateModel model)
    {
        var rate = CurrencyRate.Create(
            model.FromCurrency,
            model.ToCurrency,
            model.Rate,
            model.EffectiveAt,
            model.Source);

        await _uow.CurrencyRates.AddAsync(rate);
        await _uow.SaveChangesAsync();

        _cache.Invalidate(model.FromCurrency, model.ToCurrency);

        _logger.LogInformation(
            "Currency rate created: {From}→{To} = {Rate} (effective {EffectiveAt})",
            rate.FromCurrency, rate.ToCurrency, rate.Rate, rate.EffectiveAt);

        return rate.Id;
    }

    public async Task<decimal?> GetLatestAsync(
        string fromCurrency, string toCurrency, DateTime atTime)
    {
        var spec = new LatestCurrencyRateSpec(fromCurrency, toCurrency, atTime);
        var rate = await _uow.CurrencyRateSpecs.GetBySpecAsync(spec);
        return rate?.Rate;
    }

    public async Task<IReadOnlyList<CurrencyRateView>> GetRecentAsync(
        string? fromCurrency, string? toCurrency, int limit = 100)
    {
        var all = await _uow.CurrencyRates.GetAllAsync();

        return all
            .Where(r => string.IsNullOrWhiteSpace(fromCurrency)
                     || r.FromCurrency.Equals(fromCurrency, StringComparison.OrdinalIgnoreCase))
            .Where(r => string.IsNullOrWhiteSpace(toCurrency)
                     || r.ToCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.EffectiveAt)
            .Take(limit)
            .Select(r => new CurrencyRateView(
                r.Id, r.FromCurrency, r.ToCurrency, r.Rate,
                r.EffectiveAt, r.Source, r.CreatedAt))
            .ToList();
    }
}