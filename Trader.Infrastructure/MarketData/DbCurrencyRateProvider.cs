using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.MarketData;

public sealed class DbCurrencyRateProvider : ICurrencyRateProvider
{
    private static readonly TimeSpan RecentWindow = TimeSpan.FromHours(1);

    private readonly CurrencyRateCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DbCurrencyRateProvider> _logger;

    public DbCurrencyRateProvider(
        CurrencyRateCache cache,
        IServiceScopeFactory scopeFactory,
        ILogger<DbCurrencyRateProvider> logger)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<decimal> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateTime atTime,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
            throw new ArgumentException("FromCurrency is required.", nameof(fromCurrency));

        if (string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("ToCurrency is required.", nameof(toCurrency));

        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();

        // ─── Identity pair ───
        if (from == to) return 1m;

        var utcAtTime = atTime.Kind == DateTimeKind.Utc
            ? atTime
            : DateTime.SpecifyKind(atTime, DateTimeKind.Utc);

        var isRecent = (DateTime.UtcNow - utcAtTime) <= RecentWindow;

        // ─── مسیر ۱: نرخ‌های اخیر → از Cache ───
        if (isRecent)
        {
            var cached = await _cache.TryGetOrLoadLatestAsync(from, to, ct);
            if (cached is not null)
                return cached.Value;
        }

        // ─── مسیر ۲: DB — Pair مستقیم ───
        var directRate = await QueryDbAsync(from, to, utcAtTime, ct);
        if (directRate is not null)
            return directRate.Value;

        // ─── مسیر ۳: DB — Pair معکوس (invert) ───
        var reverseRate = await QueryDbAsync(to, from, utcAtTime, ct);
        if (reverseRate is not null)
        {
            if (reverseRate.Value == 0m)
                throw new InvalidOperationException(
                    $"Reverse rate {to}→{from} is zero; cannot invert.");

            var inverted = 1m / reverseRate.Value;
            _logger.LogDebug(
                "Using inverted rate: {To}→{From}={Reverse} → {From}→{To}={Inverted}",
                to, from, reverseRate.Value, inverted);
            return inverted;
        }

        throw new CurrencyRateNotFoundException(
            $"No currency rate found for {from}→{to} at or before {utcAtTime:u}. " +
            $"Add a rate via API or DB.");
    }

    private async Task<decimal?> QueryDbAsync(
        string from, string to, DateTime atTime, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var spec = new LatestCurrencyRateSpec(from, to, atTime);
        var rate = await uow.CurrencyRateSpecs.GetBySpecAsync(spec);

        if (rate is null)
            return null;

        _logger.LogDebug(
            "DB rate: {From}→{To} = {Rate} (effective {EffectiveAt}, queried at {AtTime})",
            from, to, rate.Rate, rate.EffectiveAt, atTime);

        return rate.Rate;
    }
}

/// <summary>نرخ ارز در DB موجود نیست.</summary>
public sealed class CurrencyRateNotFoundException : Exception
{
    public CurrencyRateNotFoundException(string message) : base(message) { }
}