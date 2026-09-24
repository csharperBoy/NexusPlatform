namespace Trader.Application.Dtos
{
    public class MarketSymbolInfoView
    {
        public string SymbolIsin { get; set; } = default!;
        public long? HighAllowedPrice { get; set; }
        public long? LowAllowedPrice { get; set; }
        public long? LastTradedPrice { get; set; }
        public long? ClosingPrice { get; set; }
        public long? FirstTradedPrice { get; set; }
        public string? TradeDate { get; set; }
        public long FetchedAt { get; set; }
    }
}