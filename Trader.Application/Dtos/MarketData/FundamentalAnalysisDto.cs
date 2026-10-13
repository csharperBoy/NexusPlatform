namespace Trader.Application.Dtos.MarketData;

public class FundamentalAnalysisDto
{
    public double TotalScore { get; set; }
    public List<FundamentalCategoryScoreDto> Categories { get; set; } = new();
}

public class FundamentalCategoryScoreDto
{
    /// <summary>عنوان فارسی که کارگزاری می‌ده — خام نگه‌داری می‌شه.</summary>
    public string Title { get; set; } = "";

    /// <summary>امتیاز ۰ تا ۱۰.</summary>
    public double Score { get; set; }
}

public static class FundamentalCategories
{
    public const string Profitability = "سودآوری";
    public const string RelativeValuation = "ارزش گذاری نسبی";
    public const string FinancialRisk = "ریسک مالی";
    public const string Efficiency = "کارایی";
    public const string MonthlySales = "فروش ماهانه";
    public const string MarketRisk = "ریسک بازار";
    public const string Momentum = "مومنتوم";
    public const string ShareholdersPerformance = "عملکرد سهامداران";
    public const string Liquidity = "نقد شوندگی";
}