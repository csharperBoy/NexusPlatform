namespace Trader.Application.Dtos.MarketData;

public class BatchMarketDataItemDto
{
    public string SymbolIsin { get; set; } = "";
    public string StateCode { get; set; } = "";

    public long LastTradedPrice { get; set; }
    public long ClosingPrice { get; set; }
    public long FeeOfPreviousDaysClosingPrice { get; set; }
    public double PriceVar { get; set; }

    public decimal BuyRatio { get; set; }
    public decimal SellRatio { get; set; }

    public long HighPrice { get; set; }
    public long LowPrice { get; set; }
    public long FirstTradedPrice { get; set; }
    public long LowAllowedPrice { get; set; }
    public long HighAllowedPrice { get; set; }

    public long BestBuyPrice { get; set; }
    public long BestBuyQuantity { get; set; }
    public long BestSellPrice { get; set; }
    public long BestSellQuantity { get; set; }

    public long TotalTradeValue { get; set; }
    public long TotalNumberOfTrades { get; set; }
    public long TotalNumberOfSharesTraded { get; set; }

    public long MinValidBuyVolume { get; set; }
    public long MaxValidBuyVolume { get; set; }
    public long MinValidSellVolume { get; set; }
    public long MaxValidSellVolume { get; set; }
    public long PriceTickSize { get; set; }
}