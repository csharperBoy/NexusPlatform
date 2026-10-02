namespace Trader.Application.Dtos.Finance;

public class PaymentAccountBalancesDto
{
    public long TotalBalanceRial { get; set; }
    public bool IsCustomerConstraintRestricted { get; set; }
    public List<BankAccountDto> BankAccounts { get; set; } = new();
    public List<AccountBalancePerDateDto> BalancesPerDate { get; set; } = new();
}