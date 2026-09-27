namespace Trader.Application.Dtos
{
    public class SymbolMarketDataDto
    {
        public string SymbolName { get; set; } = default!;
        public long? HighAllowedPrice { get; set; }
        public long? LowAllowedPrice { get; set; }
        public long? LastTradedPrice { get; set; }
        public long? ClosingPrice { get; set; }
        public long? FirstTradedPrice { get; set; }
        public string? TradeDate { get; set; }
        public long FetchedAt { get; set; }

        /// <summary>فیلدهای اضافی مخصوص هر broker</summary>
        public Dictionary<string, object?>? Extra { get; set; }
    }
}