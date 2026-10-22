using Trader.Application.Runtime;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.MarketData;

/// <summary>
/// تبدیل بین Runtime MarketSnapshot و Domain MarketSnapshotRecord.
/// Domain به Application وابسته نمی‌شود — این Mapper در Infrastructure است.
/// </summary>
public static class MarketSnapshotRecordMapper
{
    public static MarketSnapshotRecord FromRuntime(MarketSnapshot src)
    {
        return MarketSnapshotRecord.Create(
            isin: src.Isin,
            symbolName: src.SymbolName,
            capturedAt: src.CapturedAt,
            stateCode: src.StateCode,
            lastTradedPrice: src.LastTradedPrice,
            closingPrice: src.ClosingPrice,
            previousDayClosingPrice: src.PreviousDayClosingPrice,
            priceVar: src.PriceVar,
            firstTradedPrice: src.FirstTradedPrice,
            highPrice: src.HighPrice,
            lowPrice: src.LowPrice,
            highAllowedPrice: src.HighAllowedPrice,
            lowAllowedPrice: src.LowAllowedPrice,
            priceTickSize: src.PriceTickSize,
            bestBuyPrice: src.BestBuyPrice,
            bestBuyQuantity: src.BestBuyQuantity,
            bestSellPrice: src.BestSellPrice,
            bestSellQuantity: src.BestSellQuantity,
            buyRatio: src.BuyRatio,
            sellRatio: src.SellRatio,
            minValidBuyVolume: src.MinValidBuyVolume,
            maxValidBuyVolume: src.MaxValidBuyVolume,
            minValidSellVolume: src.MinValidSellVolume,
            maxValidSellVolume: src.MaxValidSellVolume,
            totalTradeValue: src.TotalTradeValue,
            totalNumberOfTrades: src.TotalNumberOfTrades,
            totalNumberOfSharesTraded: src.TotalNumberOfSharesTraded,
            source: src.Source);
    }

    public static MarketSnapshot ToRuntime(MarketSnapshotRecord src)
    {
        return new MarketSnapshot(
            Id: src.Id,
            Isin: src.Isin,
            SymbolName: src.SymbolName,
            StateCode: src.StateCode,
            LastTradedPrice: src.LastTradedPrice,
            ClosingPrice: src.ClosingPrice,
            PreviousDayClosingPrice: src.PreviousDayClosingPrice,
            PriceVar: src.PriceVar,
            FirstTradedPrice: src.FirstTradedPrice,
            HighPrice: src.HighPrice,
            LowPrice: src.LowPrice,
            HighAllowedPrice: src.HighAllowedPrice,
            LowAllowedPrice: src.LowAllowedPrice,
            PriceTickSize: src.PriceTickSize,
            BestBuyPrice: src.BestBuyPrice,
            BestBuyQuantity: src.BestBuyQuantity,
            BestSellPrice: src.BestSellPrice,
            BestSellQuantity: src.BestSellQuantity,
            BuyRatio: src.BuyRatio,
            SellRatio: src.SellRatio,
            MinValidBuyVolume: src.MinValidBuyVolume,
            MaxValidBuyVolume: src.MaxValidBuyVolume,
            MinValidSellVolume: src.MinValidSellVolume,
            MaxValidSellVolume: src.MaxValidSellVolume,
            TotalTradeValue: src.TotalTradeValue,
            TotalNumberOfTrades: src.TotalNumberOfTrades,
            TotalNumberOfSharesTraded: src.TotalNumberOfSharesTraded,
            CapturedAt: src.CapturedAt,
            Source: src.Source);
    }
}