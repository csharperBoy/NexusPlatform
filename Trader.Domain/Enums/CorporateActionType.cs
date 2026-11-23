namespace Trader.Domain.Enums;

public enum CorporateActionType
{
    /// <summary>سود نقدی — factor = (P_before - amount) / P_before</summary>
    Dividend = 0,

    /// <summary>افزایش سرمایه از سود انباشته/تجدید ارزیابی (free shares) — factor = 1 / (1 + ratio)</summary>
    BonusShares = 1,

    /// <summary>افزایش سرمایه از آورده نقدی (paid rights) — factor = (P + subPrice×ratio) / (P×(1+ratio))</summary>
    RightsIssue = 2,
}