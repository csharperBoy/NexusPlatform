namespace Trader.Infrastructure.CorporateActions;

public sealed class CorporateActionsOptions
{
    public const string SectionName = "Trader:CorporateActions";

    /// <summary>
    /// حساب EasyTrader فعال که برای fetch CA استفاده می‌شه.
    /// اگر null/empty → CA collection skip می‌شه (candles همچنان کار می‌کنن).
    /// </summary>
    public Guid? DataAccountId { get; set; }

    /// <summary>نرخ مالیات سود نقدی (0-1). پیش‌فرض 0 (ایران معاف).</summary>
    public decimal DividendTaxRate { get; set; } = 0.0m;

    /// <summary>اگر true، تمام Capital events = BonusShares (تا زمانی که RightsIssue از text تشخیص داده بشه).</summary>
    public bool RightsIssueAssumeBonus { get; set; } = true;

    /// <summary>اگر false، Backtest/Tuning به‌طور پیش‌فرض تعدیل نمی‌کنن.</summary>
    public bool AdjustmentDefaultEnabled { get; set; } = true;
}