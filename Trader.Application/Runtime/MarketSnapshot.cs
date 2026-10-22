namespace Trader.Application.Runtime;

/// <summary>
/// نمای لحظه‌ای یک Instrument. Immutable.
/// Id بعد از Persist توسط Stage 1 پر می‌شود (null قبل از Save).
/// </summary>
public sealed record MarketSnapshot(
    // ─── Identity (nullable در حافظه، بعد از Save پر می‌شود) ───
    Guid? Id,
    string Isin,
    string? SymbolName,

    // ─── Market State ───
    string StateCode,

    // ─── Price ───
    long LastTradedPrice,
    long ClosingPrice,
    long PreviousDayClosingPrice,
    double PriceVar,
    long FirstTradedPrice,
    long HighPrice,
    long LowPrice,
    long HighAllowedPrice,
    long LowAllowedPrice,
    long PriceTickSize,

    // ─── Order Book (Top of Book) ───
    long BestBuyPrice,
    long BestBuyQuantity,
    long BestSellPrice,
    long BestSellQuantity,

    // ─── Pressure ───
    decimal BuyRatio,
    decimal SellRatio,
    long MinValidBuyVolume,
    long MaxValidBuyVolume,
    long MinValidSellVolume,
    long MaxValidSellVolume,

    // ─── Trade Stats ───
    long TotalTradeValue,
    long TotalNumberOfTrades,
    long TotalNumberOfSharesTraded,

    // ─── Meta ───
    DateTime CapturedAt,
    string Source);