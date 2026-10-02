namespace Trader.Application.Dtos.Finance;

public class AccountBalancePerDateDto
{
    public int EffectiveDate { get; set; }          // 0=امروز, 1=فردا, ...
    public DateOnly PerformDate { get; set; }
    public long AvailableBalanceRial { get; set; }
    public long MaxSingleRequestAmountRial { get; set; }
    public long MaxTotalRequestAmountRial { get; set; }
    public int? MaxTotalRequestCount { get; set; }
    public bool HasImeWallet { get; set; }
    public List<AccountBalancePerBankDto> PerBank { get; set; } = new();
}