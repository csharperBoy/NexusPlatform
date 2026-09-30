namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// بازدهی — برای صندوق‌ها و اوراق
    /// </summary>
    public class ReturnChartDto
    {
        public long LastTradedPrice { get; set; }

        /// <summary>بازدهی ۳۰ روز</summary>
        public decimal E30 { get; set; }

        /// <summary>بازدهی ۹۰ روز</summary>
        public decimal E90 { get; set; }

        /// <summary>بازدهی ۳۶۰ روز</summary>
        public decimal E360 { get; set; }

        public string? MaturityDay { get; set; }
        public int? DaysToMaturity { get; set; }
        public decimal? ReturnToMaturity { get; set; }
    }
}