using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

public sealed class BacktestDataCache : IBacktestDataCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICandleTransformer _transformer;

    public BacktestDataCache(
        IServiceScopeFactory scopeFactory,
        ICandleTransformer transformer)
    {
        _scopeFactory = scopeFactory;
        _transformer = transformer;
    }
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;

    private readonly ConcurrentDictionary<(string, TimeFrame, DateTime, DateTime),
        Lazy<Task<IReadOnlyList<Candle>>>> _candles = new();

    private readonly ConcurrentDictionary<(string, TimeFrame, DateTime, DateTime, string),
        Lazy<Task<IReadOnlyList<Candle>>>> _transformed = new();

    private readonly ConcurrentDictionary<string, Lazy<Task<Instrument>>> _instruments = new();

    

    public Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string isin, TimeFrame timeFrame,
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var key = (isin, timeFrame, from, to);
        return _candles
            .GetOrAdd(key, k => new Lazy<Task<IReadOnlyList<Candle>>>(
                () => LoadCandlesAsync(k.Item1, k.Item2, k.Item3, k.Item4, ct)))
            .Value;
    }

    public Task<IReadOnlyList<Candle>> GetTransformedCandlesAsync(
        string isin, TimeFrame timeFrame,
        DateTime from, DateTime to, string targetCurrency,
        CancellationToken ct = default)
    {
        var key = (isin, timeFrame, from, to, targetCurrency.ToUpperInvariant());
        return _transformed
            .GetOrAdd(key, k => new Lazy<Task<IReadOnlyList<Candle>>>(
                () => LoadAndTransformAsync(
                    k.Item1, k.Item2, k.Item3, k.Item4, k.Item5, ct)))
            .Value;
    }

    public Task<Instrument> GetInstrumentAsync(string isin, CancellationToken ct = default)
    {
        return _instruments
            .GetOrAdd(isin, k => new Lazy<Task<Instrument>>(
                () => LoadInstrumentAsync(k, ct)))
            .Value;
    }

    /* ═══════════════════ Internal ═══════════════════ */


    private async Task<IReadOnlyList<Candle>> LoadCandlesAsync(
        string isin, TimeFrame tf, DateTime from, DateTime to, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var rows = await uow.Context.Candles
            .AsNoTracking()
            .Where(c => c.Isin == isin
                     && (int)c.TimeFrame == (int)tf
                     && c.OpenTime >= from
                     && c.OpenTime <= to)
            .OrderBy(c => c.OpenTime)
            .ToListAsync(ct);
        return rows.Select(r => new Candle(
            r.OpenTime, r.Open, r.High, r.Low, r.Close, r.Volume)).ToList();
    }

    private async Task<Instrument> LoadInstrumentAsync(string isin, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var inst = await uow.InstrumentSpecs.GetBySpecAsync(new InstrumentByIsinSpec(isin));
        return inst ?? throw new InvalidOperationException(
            $"Instrument '{isin}' not found.");
    }

    private async Task<IReadOnlyList<Candle>> LoadAndTransformAsync(
        string isin, TimeFrame tf, DateTime from, DateTime to,
        string targetCurrency, CancellationToken ct)
    {
        var raw = await GetCandlesAsync(isin, tf, from, to, ct);
        if (raw.Count == 0) return Array.Empty<Candle>();

        var tr = await _transformer.TransformAsync(isin, raw, targetCurrency, to, ct);
        return tr.Candles;
    }

}