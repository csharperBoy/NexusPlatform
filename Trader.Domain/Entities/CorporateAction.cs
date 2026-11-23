using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

public class CorporateAction : BaseEntity, IAuditableEntity
{
    public string Isin { get; private set; } = default!;
    public DateTime ExDateUtc { get; private set; }
    public CorporateActionType ActionType { get; private set; }

    /// <summary>برای Dividend: مبلغ به ازای هر سهم (ریال)</summary>
    public decimal? Amount { get; private set; }

    /// <summary>برای BonusShares/RightsIssue: درصد (0.25 = 25%)</summary>
    public decimal? Ratio { get; private set; }

    /// <summary>برای RightsIssue: قیمت حق تقدم (اختیاری — MVP null)</summary>
    public decimal? SubscriptionPrice { get; private set; }

    /// <summary>توضیح انسانی (strip شده از HTML)</summary>
    public string? Description { get; private set; }

    /// <summary>متن اصلی HTML — audit trail</summary>
    public string? RawText { get; private set; }

    /// <summary>شناسه‌ی API منبع (مثلاً "dividend20210721") — برای dedupe</summary>
    public string? SourceId { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private CorporateAction() { }

    public static CorporateAction Create(
        string isin,
        DateTime exDateUtc,
        CorporateActionType actionType,
        decimal? amount,
        decimal? ratio,
        decimal? subscriptionPrice,
        string? description,
        string? rawText,
        string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(isin))
            throw new ArgumentException("Isin required.", nameof(isin));

        return new CorporateAction
        {
            Isin = isin.Trim(),
            ExDateUtc = DateTime.SpecifyKind(exDateUtc, DateTimeKind.Utc),
            ActionType = actionType,
            Amount = amount,
            Ratio = ratio,
            SubscriptionPrice = subscriptionPrice,
            Description = description,
            RawText = rawText,
            SourceId = sourceId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// ضریب تعدیل قیمت‌های قبل از ExDate. مقدار 1m = بدون تعدیل. null = غیرقابل محاسبه.
    /// candle_adi = candle_raw × factor برای همه‌ی candles با OpenTime &lt; ExDateUtc.
    /// </summary>
    public decimal? ComputeAdjustmentFactor(decimal priceBeforeEx)
    {
        if (priceBeforeEx <= 0m) return null;

        return ActionType switch
        {
            CorporateActionType.Dividend when Amount.HasValue =>
                (priceBeforeEx - Amount.Value) / priceBeforeEx,

            CorporateActionType.BonusShares when Ratio.HasValue =>
                1m / (1m + Ratio.Value),

            CorporateActionType.RightsIssue when Ratio.HasValue && SubscriptionPrice.HasValue =>
                (priceBeforeEx + SubscriptionPrice.Value * Ratio.Value)
                / (priceBeforeEx * (1m + Ratio.Value)),

            _ => 1m,
        };
    }
}