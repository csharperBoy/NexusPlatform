namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// خلاصه‌ی سفارش‌های بازار (صف)
    /// </summary>
    public class MarketSheetSumDto
    {
        public long BuyVolume { get; set; }
        public long BuyCount { get; set; }
        public long SellVolume { get; set; }
        public long SellCount { get; set; }
    }
}