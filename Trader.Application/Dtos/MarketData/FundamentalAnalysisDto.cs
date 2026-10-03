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