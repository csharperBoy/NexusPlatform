namespace Trader.Application.Dtos.Finance;

public class PaymentCancelResultDto
{
    /// <summary>شناسه‌ی درخواستی که کنسل شد.</summary>
    public long PaymentId { get; set; }

    /// <summary>
    /// موفقیت یا شکست.
    /// ⚠️ کارگزاری body برنمی‌گردونه — این فقط بر اساس status code هست.
    /// برای تأیید نهایی، بعدش GetPaymentHistoryAsync رو صدا بزن و state رو چک کن.
    /// </summary>
    public bool IsSuccessful { get; set; }

    public int StatusCode { get; set; }
    public string? ErrorBody { get; set; }

    public long FireAtUnixMs { get; set; }
    public long ReceivedAtUnixMs { get; set; }
    public long LatencyMs => ReceivedAtUnixMs - FireAtUnixMs;
}