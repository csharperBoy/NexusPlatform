using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// فروشگاه In-memory آخرین Snapshotها. Singleton، Thread-safe.
/// Stage 2 فقط از این می‌خواند، نه از Provider.
/// </summary>
public interface IMarketSnapshotStore
{
    void Upsert(MarketSnapshot snapshot);
    void UpsertRange(IReadOnlyList<MarketSnapshot> snapshots);

    MarketSnapshot? GetLatest(string isin);
    IReadOnlyList<MarketSnapshot> GetLatestBatch(IReadOnlyList<string> isins);

    /// <summary>حذف نمادهایی که در هیچ Run فعالی نیستند.</summary>
    void RetainOnly(IReadOnlySet<string> activeIsins);

    int Count { get; }
}