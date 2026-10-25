using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.MarketData;

/// <summary>
/// کش «آخرین نرخ» per pair. TTL ۵ دقیقه.
/// فقط برای نرخ‌های اخیر (atTime ≥ now - 1h) استفاده می‌شود.
/// Historical queries به DB می‌روند.
/// </summary>
public sealed class CurrencyRateCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private sealed record Entry(decimal Rate, DateTime LoadedAt);

    private readonly ConcurrentDictionary<string, Entry> _cache = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurrencyRateCache> _logger;

    public CurrencyRateCache(
        IServiceScopeFactory scopeFactory,
        ILogger<CurrencyRateCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<decimal?> TryGetOrLoadLatestAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken ct = default)
    {
        var key = BuildKey(fromCurrency, toCurrency);

        // ─── Cache hit ───
        if (_cache.TryGetValue(key, out var entry)
            && DateTime.UtcNow - entry.LoadedAt < Ttl)
        {
            return entry.Rate;
        }

        // ─── Cache miss → DB ───
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var spec = new LatestCurrencyRateSpec(
                fromCurrency, toCurrency, DateTime.UtcNow);
            var rate = await uow.CurrencyRateSpecs.GetBySpecAsync(spec);

            if (rate is null)
            {
                _logger.LogDebug(
                    "No recent rate for {From}→{To}.", fromCurrency, toCurrency);
                return null;
            }

            _cache[key] = new Entry(rate.Rate, DateTime.UtcNow);

            _logger.LogDebug(
                "Currency rate cached: {From}→{To} = {Rate} (effective {EffectiveAt})",
                fromCurrency, toCurrency, rate.Rate, rate.EffectiveAt);

            return rate.Rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load rate for {From}→{To}.", fromCurrency, toCurrency);
            return null;
        }
    }

    public void Invalidate(string fromCurrency, string toCurrency)
        => _cache.TryRemove(BuildKey(fromCurrency, toCurrency), out _);

    public void InvalidateAll() => _cache.Clear();

    private static string BuildKey(string from, string to)
        => $"{from.ToUpperInvariant()}:{to.ToUpperInvariant()}";
}