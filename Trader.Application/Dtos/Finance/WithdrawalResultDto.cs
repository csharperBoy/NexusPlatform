namespace Trader.Application.Dtos.Finance;

public class WithdrawalResultDto
{
    /// <summary>
    /// موفقیت یا شکست بر اساس status code.
    /// ⚠️ کارگزاری body برنمی‌گردونه — پس این تنها سیگنال موفقیت هست.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>Status code کارگزاری (200, 400, 500, ...).</summary>
    public int StatusCode { get; set; }

    /// <summary>اگه شکست خورده، متن خطا (اگه body داشت).</summary>
    public string? ErrorBody { get; set; }

    public long FireAtUnixMs { get; set; }
    public long ReceivedAtUnixMs { get; set; }
    public long LatencyMs => ReceivedAtUnixMs - FireAtUnixMs;
}