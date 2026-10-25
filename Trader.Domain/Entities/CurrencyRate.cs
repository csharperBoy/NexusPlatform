using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// نرخ برابری ارز در یک لحظه‌ی مشخص.
/// Time-series: می‌توان چند ردیف برای یک Pair با EffectiveAt مختلف داشت.
/// Rate = مقدار ToCurrency برای ۱ واحد FromCurrency.
/// مثال: USD→IRR با Rate=650,000 یعنی ۱ دلار = ۶۵۰,۰۰۰ ریال.
/// </summary>
public class CurrencyRate : BaseEntity , IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public string FromCurrency { get; private set; } = default!;   // "USD"
    public string ToCurrency { get; private set; } = default!;     // "IRR"
    public decimal Rate { get; private set; }                       // 650000

    /// <summary>زمان اعتبار نرخ (UTC). آخرین نرخ با EffectiveAt ≤ atTime استفاده می‌شود.</summary>
    public DateTime EffectiveAt { get; private set; }

    /// <summary>منبع نرخ: "Manual", "Nima", "Sana", ...</summary>
    public string Source { get; private set; } = "Manual";

    private CurrencyRate() { } // EF

    public static CurrencyRate Create(
        string fromCurrency,
        string toCurrency,
        decimal rate,
        DateTime effectiveAt,
        string source = "Manual")
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
            throw new ArgumentException("FromCurrency is required.", nameof(fromCurrency));

        if (string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("ToCurrency is required.", nameof(toCurrency));

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "FromCurrency and ToCurrency must be different.");

        if (rate <= 0m)
            throw new ArgumentOutOfRangeException(nameof(rate),
                "Rate must be greater than zero.");

        return new CurrencyRate
        {
            FromCurrency = fromCurrency.Trim().ToUpperInvariant(),
            ToCurrency = toCurrency.Trim().ToUpperInvariant(),
            Rate = rate,
            EffectiveAt = effectiveAt.Kind == DateTimeKind.Utc
                ? effectiveAt
                : DateTime.SpecifyKind(effectiveAt, DateTimeKind.Utc),
            Source = source.Trim()
        };
    }

    public void UpdateRate(decimal rate, DateTime effectiveAt, string source)
    {
        if (rate <= 0m)
            throw new ArgumentOutOfRangeException(nameof(rate));

        Rate = rate;
        EffectiveAt = effectiveAt.Kind == DateTimeKind.Utc
            ? effectiveAt
            : DateTime.SpecifyKind(effectiveAt, DateTimeKind.Utc);
        Source = source.Trim();
    }
}