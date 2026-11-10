using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// یک Source پیکربندی‌شده برای جمع‌آوری نرخ (D34).
/// مثال: "tgju-usd" با Type=Tgju و ConfigJson={"indicator":"price_dollar_rl"}.
///
/// افزودن Source جدید (Indicator جدید یا Provider جدید) = یک رکورد در DB، صفر تغییر کد.
/// </summary>
public class PriceSource : BaseEntity, IAuditableEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public PriceSourceType Type { get; private set; }
    public string ConfigJson { get; private set; } = default!;
    public string FromAsset { get; private set; } = default!;
    public string ToAsset { get; private set; } = default!;
    public bool IsActive { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private PriceSource() { } // EF

    public static PriceSource Create(
        string code,
        string name,
        PriceSourceType type,
        string configJson,
        string fromAsset,
        string toAsset)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(configJson))
            throw new ArgumentException("ConfigJson is required.", nameof(configJson));
        if (string.IsNullOrWhiteSpace(fromAsset))
            throw new ArgumentException("FromAsset is required.", nameof(fromAsset));
        if (string.IsNullOrWhiteSpace(toAsset))
            throw new ArgumentException("ToAsset is required.", nameof(toAsset));

        var from = fromAsset.Trim().ToUpperInvariant();
        var to = toAsset.Trim().ToUpperInvariant();

        if (from == to)
            throw new InvalidOperationException(
                "FromAsset and ToAsset must be different.");

        return new PriceSource
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Type = type,
            ConfigJson = configJson,
            FromAsset = from,
            ToAsset = to,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string name,
        string configJson,
        string fromAsset,
        string toAsset)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(configJson))
            throw new ArgumentException("ConfigJson is required.", nameof(configJson));

        var from = fromAsset.Trim().ToUpperInvariant();
        var to = toAsset.Trim().ToUpperInvariant();

        if (from == to)
            throw new InvalidOperationException(
                "FromAsset and ToAsset must be different.");

        Name = name.Trim();
        ConfigJson = configJson;
        FromAsset = from;
        ToAsset = to;
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
}