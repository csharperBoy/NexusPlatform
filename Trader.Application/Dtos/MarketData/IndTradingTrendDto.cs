namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// روند معاملات حقیقی (چند روز گذشته)
    /// </summary>
    public class IndTradingTrendDto
    {
        /// <summary>
        /// "individualbuypower" | "netindividual" |
        /// "netindividualvalue" | "percapita"
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>تاریخ شمسی "1405/07/08"</summary>
        public string Date { get; set; } = default!;

        public decimal Val { get; set; }
    }
}