using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.MarketData;

public sealed class InMemoryMarketSnapshotStore : IMarketSnapshotStore
{
    private readonly ConcurrentDictionary<string, MarketSnapshot> _store = new();
    private readonly ILogger<InMemoryMarketSnapshotStore> _logger;

    public InMemoryMarketSnapshotStore(ILogger<InMemoryMarketSnapshotStore> logger)
    {
        _logger = logger;
    }

    public int Count => _store.Count;

    public void Upsert(MarketSnapshot snapshot)
    {
        _store[snapshot.Isin] = snapshot;
    }

    public void UpsertRange(IReadOnlyList<MarketSnapshot> snapshots)
    {
        foreach (var s in snapshots)
            _store[s.Isin] = s;

        if (snapshots.Count > 0)
            _logger.LogDebug("Upserted {Count} snapshots. Total: {Total}",
                snapshots.Count, _store.Count);
    }

    public MarketSnapshot? GetLatest(string isin)
        => _store.TryGetValue(isin, out var s) ? s : null;

    public IReadOnlyList<MarketSnapshot> GetLatestBatch(IReadOnlyList<string> isins)
    {
        var result = new List<MarketSnapshot>(isins.Count);
        foreach (var isin in isins)
            if (_store.TryGetValue(isin, out var s))
                result.Add(s);
        return result;
    }

    public void RetainOnly(IReadOnlySet<string> activeIsins)
    {
        var toRemove = _store.Keys.Where(k => !activeIsins.Contains(k)).ToList();
        foreach (var k in toRemove)
            _store.TryRemove(k, out _);

        if (toRemove.Count > 0)
            _logger.LogDebug("Removed {Count} stale snapshots. Total: {Total}",
                toRemove.Count, _store.Count);
    }
}