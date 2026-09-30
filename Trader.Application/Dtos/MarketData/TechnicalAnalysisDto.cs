namespace Trader.Application.Dtos.MarketData
{
    /// <summary>
    /// سیگنال تحلیل تکنیکال آماده از کارگزاری
    /// </summary>
    public class TechnicalAnalysisDto
    {
        public TechnicalScoreDto TotalScore { get; set; } = new();
        public List<TechnicalCategoryScoreDto> CategoryScore { get; set; } = new();
    }

    public class TechnicalScoreDto
    {
        /// <summary>"totalscore"</summary>
        public string Cat { get; set; } = default!;

        /// <summary>مقدار عددی سیگنال</summary>
        public decimal Value { get; set; }

        /// <summary>"Buy" | "StrongBuy" | "Sell" | "StrongSell" | "Neutral"</summary>
        public string State { get; set; } = default!;
    }

    public class TechnicalCategoryScoreDto
    {
        /// <summary>"indicator" | "movingaverage"</summary>
        public string Cat { get; set; } = default!;

        /// <summary>ترجمه فارسی</summary>
        public string CatFa { get; set; } = default!;

        public string State { get; set; } = default!;
    }
}