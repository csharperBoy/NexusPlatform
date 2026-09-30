namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// حجم/تعداد خرید و فروش حقیقی و حقوقی (لحظه‌ای)
    /// </summary>
    public class IndInstTradeDto
    {
        public string? SymbolIsin { get; set; }
        public long IndBuyVolume { get; set; }
        public long IndBuyNumber { get; set; }
        public long IndSellVolume { get; set; }
        public long IndSellNumber { get; set; }
        public long InsBuyVolume { get; set; }
        public long InsBuyNumber { get; set; }
        public long InsSellVolume { get; set; }
        public long InsSellNumber { get; set; }
        public string? Date { get; set; }
    }
}