namespace Trader.Application.Runtime;

/// <summary>
/// نمای لحظه‌ای یک Instrument. Immutable. تنها چیزی که Stage 1 → Stage 2 منتقل می‌شود.
/// </summary>
public sealed record MarketSnapshot(
    string Isin,
    decimal LastPrice,
    decimal ClosePrice,
    decimal OpenPrice,
    decimal HighPrice,
    decimal LowPrice,
    long Volume,
    decimal BestBidPrice,
    long BestBidVolume,
    decimal BestAskPrice,
    long BestAskVolume,
    long OpenInterest,
    DateTime CapturedAt,
    string Source);