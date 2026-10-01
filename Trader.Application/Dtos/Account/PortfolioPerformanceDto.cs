namespace Trader.Application.Dtos.Account
{
    /// <summary>
    /// عملکرد یک نماد در پرتفوی
    /// </summary>
    public class PortfolioPerformanceDto
    {
        public string SymbolIsin { get; set; } = default!;
        public string SymbolName { get; set; } = default!;

        /// <summary>تاریخ میلادی آخرین معامله</summary>
        public string Date { get; set; } = default!;

        /// <summary>تاریخ شمسی (مثل 14050704)</summary>
        public long PersianDate { get; set; }

        /// <summary>دارایی فعلی (تعداد سهم)</summary>
        public long Asset { get; set; }

        public long BonusShareVolume { get; set; }
        public long StockRightVolume { get; set; }

        /* ─── DPS (سود نقدی) ─── */
        public long PeriodicDPSAmount { get; set; }
        public long LastDPSAmount { get; set; }
        public long TotalDPSAmount { get; set; }

        /* ─── نقطه سر به سر ─── */
        public decimal BreakevenPoint { get; set; }
        public decimal BreakevenWithoutDps { get; set; }

        /* ─── دوره جاری ─── */
        public decimal PeriodicBuyAveragePrice { get; set; }
        public decimal PeriodicBuyAveragePriceWithoutDps { get; set; }
        public decimal PeriodicBuyAmount { get; set; }
        public decimal PeriodicSellAveragePrice { get; set; }
        public decimal PeriodicSellAmount { get; set; }
        public long PeriodicBoughtVolume { get; set; }
        public long PeriodicSoldVolume { get; set; }
        public decimal PeriodicRealizedProfitAndLoss { get; set; }
        public decimal PeriodicRealizedProfitAndLossWithoutDps { get; set; }

        /* ─── کل دوره ─── */
        public decimal TotalBuyAveragePrice { get; set; }
        public decimal TotalBuyAveragePriceWithoutDps { get; set; }
        public decimal TotalBuyAmount { get; set; }
        public decimal TotalSellAveragePrice { get; set; }
        public decimal TotalSellAmount { get; set; }
        public long TotalBoughtVolume { get; set; }
        public long TotalSoldVolume { get; set; }
        public decimal TotalRealizedProfitAndLoss { get; set; }
        public decimal TotalRealizedProfitAndLossWithoutDps { get; set; }

        /* ─── روز ─── */
        public decimal DailyBuyAveragePrice { get; set; }
        public decimal DailySellAveragePrice { get; set; }
        public long DailyBoughtVolume { get; set; }
        public long DailySoldVolume { get; set; }

        /* ─── خالص ─── */
        public decimal PeriodicBuyNetPrice { get; set; }
        public decimal PeriodicSellNetPrice { get; set; }
        public decimal TotalBuyNetPrice { get; set; }
        public decimal TotalSellNetPrice { get; set; }
    }
}