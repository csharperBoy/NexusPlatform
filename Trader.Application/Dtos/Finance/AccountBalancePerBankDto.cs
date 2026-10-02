namespace Trader.Application.Dtos.Finance;

public class AccountBalancePerBankDto
{
    public long BankAccountId { get; set; }
    public bool IsBankAvailable { get; set; }
    public bool IsRequestConstraintRestricted { get; set; }
    public string? RequestRestrictionDetail { get; set; }
    public long SingleRequestAmountRial { get; set; }
    public long TotalRequestAmountRial { get; set; }
    public int? TotalRequestCount { get; set; }
}