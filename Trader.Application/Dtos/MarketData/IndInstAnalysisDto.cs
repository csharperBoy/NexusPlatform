namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// تحلیل حقیقی/حقوقی — نسبت‌ها و قدرت خریدار
    /// </summary>
    public class IndInstAnalysisDto
    {
        public long IndBuyVol { get; set; }
        public decimal IndBuyPow { get; set; }
        public long IndSellVol { get; set; }
        public long InsSellVol { get; set; }
        public long InsBuyVol { get; set; }
        public decimal BidPres { get; set; }
        public decimal NetInd { get; set; }
        public decimal BuyPerInd { get; set; }
        public decimal SellPerInd { get; set; }
        public decimal DiffValInd { get; set; }
    }
}