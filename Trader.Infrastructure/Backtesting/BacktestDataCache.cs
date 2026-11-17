using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
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
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ICandleTransformer _transformer;

    private readonly ConcurrentDictionary<(string, TimeFrame, DateTime, DateTime),
        Lazy<Task<IReadOnlyList<Candle>>>> _candles = new();

    private readonly ConcurrentDictionary<(string, TimeFrame, DateTime, DateTime, string),
        Lazy<Task<IReadOnlyList<Candle>>>> _transformed = new();

    private readonly ConcurrentDictionary<string, Lazy<Task<Instrument>>> _instruments = new();

    public BacktestDataCache(
        ITraderUnitOfWork<TraderDbContext> uow,
        ICandleTransformer transformer)
    {
        _uow = uow;
        _transformer = transformer;
    }

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
        // Read-only، بدون tracking (هم‌ارز رفتار فعلی BacktestEngine.SimulateAsync)
        var rows = await _uow.Context.Candles
            .AsNoTracking()
            .Where(c => c.Isin == isin
                     && (int)c.TimeFrame == (int)tf
                     && c.OpenTime >= from
                     && c.OpenTime <= to)
            .OrderBy(c => c.OpenTime)
            .ToListAsync(ct);

        var list = new List<Candle>(rows.Count);
        foreach (var r in rows)
            list.Add(new Candle(
                OpenTime: r.OpenTime,
                Open: r.Open,
                High: r.High,
                Low: r.Low,
                Close: r.Close,
                Volume: r.Volume));
        return list;
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

    private async Task<Instrument> LoadInstrumentAsync(string isin, CancellationToken ct)
    {
        var inst = await _uow.InstrumentSpecs.GetBySpecAsync(
            new InstrumentByIsinSpec(isin));
        return inst ?? throw new InvalidOperationException(
            $"Instrument '{isin}' not found.");
    }
}