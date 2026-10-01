namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// ساعت معاملاتی برای یک نوع بازار
    /// </summary>
    public class TradingTimeDto
    {
        /// <summary>عنوان‌های بازار (مثلاً "سهام و حق تقدم سهام")</summary>
        public List<string> Title { get; set; } = new();

        /// <summary>روزهای هفته (1=شنبه, 7=جمعه)</summary>
        public List<int> Days { get; set; } = new();

        /// <summary>کد نوع بازار — کلید یکتا</summary>
        public int TradingTimeType { get; set; }

        /// <summary>"HH:mm"</summary>
        public string? StartPreTradingTime { get; set; }
        public string? EndPreTradingTime { get; set; }
        public string? StartTradingTime { get; set; }
        public string? EndTradingTime { get; set; }

        /// <summary>ساعت معاملات TAL (اگه داره)</summary>
        public string? StartTalTime { get; set; }
        public string? EndTalTime { get; set; }

        public bool HasTal =>
            !string.IsNullOrWhiteSpace(StartTalTime) &&
            !string.IsNullOrWhiteSpace(EndTalTime);
    }
}