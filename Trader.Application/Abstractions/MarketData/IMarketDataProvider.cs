using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.MarketData;

public interface IMarketDataProvider
{
    string ProviderName { get; }
    IReadOnlySet<MarketType> SupportedMarkets { get; }

    Task<MarketSnapshot?> GetSnapshotAsync(string isin, CancellationToken ct = default);

    Task<IReadOnlyList<MarketSnapshot>> GetBatchSnapshotsAsync(
        IReadOnlyList<string> isins, CancellationToken ct = default);

    Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string isin, TimeFrame timeFrame, int count, CancellationToken ct = default);
}