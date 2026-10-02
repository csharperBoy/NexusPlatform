namespace Trader.Application.Dtos.Finance;

public class BankAccountDto
{
    public long Id { get; set; }
    public string AccountNumber { get; set; } = "";
    public string ShebaNumber { get; set; } = "";
    public string? CardNumber { get; set; }
    public string BankName { get; set; } = "";      // enum-style: SADERAT_IRAN, TEJARAT, ...
    public string BankCode { get; set; } = "";      // 019, 018, ...
    public string BankTitle { get; set; } = "";     // فارسی: صادرات، تجارت
    public bool IsActive { get; set; }
    public bool? IsPending { get; set; }
    public bool? ShebaInquiry { get; set; }
    public string Status { get; set; } = "";         // CARD_VERIFIED_OTP, ...

    /// <summary>آیا این حساب بانکی واقعیه یا روش پرداخت کارتی (id منفی).</summary>
    public bool IsCardPayment => Id < 0;
}