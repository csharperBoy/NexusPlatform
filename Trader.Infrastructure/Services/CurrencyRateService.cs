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

    /// <summary>سقف absolute برای GetList — جلوگیری از DoS. برای همه‌ی رکوردها از CheckRange
    /// برای existence و برای داده‌ی حجیم از pagination (future) استفاده کنید.</summary>
    private const int MaxGetListLimit = 10_000;

    public async Task<IReadOnlyList<CurrencyRateView>> GetRecentAsync(
        string? fromCurrency, string? toCurrency, int limit = 100)
    {
        var takeClamped = Math.Clamp(limit, 1, MaxGetListLimit);

        // ─── SQL-side filter + order + take ───
        var rates = await _uow.CurrencyRates.GetAllAsync(q =>
        {
            IQueryable<CurrencyRate> query = q;

            if (!string.IsNullOrWhiteSpace(fromCurrency))
            {
                var f = fromCurrency.ToUpperInvariant();
                query = query.Where(r => r.FromCurrency == f);
            }

            if (!string.IsNullOrWhiteSpace(toCurrency))
            {
                var t = toCurrency.ToUpperInvariant();
                query = query.Where(r => r.ToCurrency == t);
            }

            return query.OrderByDescending(r => r.EffectiveAt).Take(takeClamped);
        });

        return rates
            .Select(r => new CurrencyRateView(
                r.Id, r.FromCurrency, r.ToCurrency, r.Rate,
                r.EffectiveAt, r.Source, r.CreatedAt))
            .ToList();
    }
    public async Task<bool> CheckRangeExistsAsync(
    string fromCurrency,
    string toCurrency,
    DateTime fromDate,
    DateTime toDate)
    {
        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();

        if (from == to) return true; // identity pair

        var fromUtc = ToUtc(fromDate);
        var toUtc = ToUtc(toDate);

        // Direct pair
        var direct = await _uow.CurrencyRateSpecs.ListBySpecAsync(
            new CurrencyRatesPairRangeSpec(from, to, fromUtc, toUtc));
        if (direct.Count() > 0) return true;

        // Reverse pair
        var reverse = await _uow.CurrencyRateSpecs.ListBySpecAsync(
            new CurrencyRatesPairRangeSpec(to, from, fromUtc, toUtc));
        return reverse.Count() > 0;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}