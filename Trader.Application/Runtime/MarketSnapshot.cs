namespace Trader.Application.Runtime;

/// <summary>
/// نمای لحظه‌ای یک Instrument برای Strategy. Immutable.
/// Stage 1 آن را از DTO کارگزاری می‌سازد.
/// </summary>
public sealed record MarketSnapshot(
    // ─── Identity ───
    string Isin,
    string? SymbolName,

    // ─── Market State ───
    /// <summary>کد وضعیت نماد (EasyTrader StateCode).</summary>
    string StateCode,

    // ─── Price ───
    long LastTradedPrice,
    long ClosingPrice,
    long PreviousDayClosingPrice,
    double PriceVar,                  // درصد تغییر
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