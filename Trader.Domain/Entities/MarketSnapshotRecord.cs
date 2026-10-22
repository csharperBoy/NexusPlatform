using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// ثبت هر Snapshot بازار در یک لحظه. برای AI/ML و Debug.
/// داده‌ی Historical Ground Truth — قابل بازسازی Featureها.
/// </summary>
public class MarketSnapshotRecord : BaseEntity
{
    public string Isin { get; private set; } = default!;
    public string? SymbolName { get; private set; }
    public DateTime CapturedAt { get; private set; }

    public string StateCode { get; private set; } = "";

    // ─── Price ───
    public long LastTradedPrice { get; private set; }
    public long ClosingPrice { get; private set; }
    public long PreviousDayClosingPrice { get; private set; }
    public double PriceVar { get; private set; }
    public long FirstTradedPrice { get; private set; }
    public long HighPrice { get; private set; }
    public long LowPrice { get; private set; }
    public long HighAllowedPrice { get; private set; }
    public long LowAllowedPrice { get; private set; }
    public long PriceTickSize { get; private set; }

    // ─── Order Book ───
    public long BestBuyPrice { get; private set; }
    public long BestBuyQuantity { get; private set; }
    public long BestSellPrice { get; private set; }
    public long BestSellQuantity { get; private set; }

    // ─── Pressure ───
    public decimal BuyRatio { get; private set; }
    public decimal SellRatio { get; private set; }
    public long MinValidBuyVolume { get; private set; }
    public long MaxValidBuyVolume { get; private set; }
    public long MinValidSellVolume { get; private set; }
    public long MaxValidSellVolume { get; private set; }

    // ─── Trade Stats ───
    public long TotalTradeValue { get; private set; }
    public long TotalNumberOfTrades { get; private set; }
    public long TotalNumberOfSharesTraded { get; private set; }

    // ─── Meta ───
    public string Source { get; private set; } = "";

    private MarketSnapshotRecord() { } // EF

    public static MarketSnapshotRecord Create(
        string isin,
        string? symbolName,
        DateTime capturedAt,
        string stateCode,
        long lastTradedPrice,
        long closingPrice,
        long previousDayClosingPrice,
        double priceVar,
        long firstTradedPrice,
        long highPrice,
        long lowPrice,
        long highAllowedPrice,
        long lowAllowedPrice,
        long priceTickSize,
        long bestBuyPrice,
        long bestBuyQuantity,
        long bestSellPrice,
        long bestSellQuantity,
        decimal buyRatio,
        decimal sellRatio,
        long minValidBuyVolume,
        long maxValidBuyVolume,
        long minValidSellVolume,
        long maxValidSellVolume,
        long totalTradeValue,
        long totalNumberOfTrades,
        long totalNumberOfSharesTraded,
        string source)
    {
        return new MarketSnapshotRecord
        {
            Isin = isin,
            SymbolName = symbolName,
            CapturedAt = capturedAt,
            StateCode = stateCode,
            LastTradedPrice = lastTradedPrice,
            ClosingPrice = closingPrice,
            PreviousDayClosingPrice = previousDayClosingPrice,
            PriceVar = priceVar,
            FirstTradedPrice = firstTradedPrice,
            HighPrice = highPrice,
            LowPrice = lowPrice,
            HighAllowedPrice = highAllowedPrice,
            LowAllowedPrice = lowAllowedPrice,
            PriceTickSize = priceTickSize,
            BestBuyPrice = bestBuyPrice,
            BestBuyQuantity = bestBuyQuantity,
            BestSellPrice = bestSellPrice,
            BestSellQuantity = bestSellQuantity,
            BuyRatio = buyRatio,
            SellRatio = sellRatio,
            MinValidBuyVolume = minValidBuyVolume,
            MaxValidBuyVolume = maxValidBuyVolume,
            MinValidSellVolume = minValidSellVolume,
            MaxValidSellVolume = maxValidSellVolume,
            TotalTradeValue = totalTradeValue,
            TotalNumberOfTrades = totalNumberOfTrades,
            TotalNumberOfSharesTraded = totalNumberOfSharesTraded,
            Source = source
        };
    }
}