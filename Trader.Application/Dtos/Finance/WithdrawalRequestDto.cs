namespace Trader.Application.Dtos.Finance;

public class WithdrawalRequestDto
{
    public long BankAccountId { get; set; }
    public string Iban { get; set; } = "";

    /// <summary>مبلغ به ریال.</summary>
    public long AmountRial { get; set; }

    public DateOnly PerformDate { get; set; }
    public bool IsImeRequest { get; set; }
}