using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// یک Kill Switch (Emergency Stop) فعال یا غیرفعال.
///
/// Scopes:
///   • Global: AccountId == null → همه‌ی Orderها Block
///   • Account: AccountId != null → فقط Orderهای آن Account Block
///
/// Lifecycle: فقط یک Record فعال برای هر (Scope, AccountId) وجود دارد.
/// غیرفعال شدن → DeactivatedAt پر می‌شود (نه Delete — برای Audit).
/// </summary>
public class RiskKillSwitch : BaseEntity, IAuditableEntity
{
    /// <summary>null = Global، non-null = per-Account.</summary>
    public Guid? AccountId { get; private set; }

    public DateTime ActivatedAt { get; private set; }
    public string? ActivatedBy { get; private set; }
    public string? Reason { get; private set; }

    public DateTime? DeactivatedAt { get; private set; }
    public string? DeactivatedBy { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private RiskKillSwitch() { } // EF

    public static RiskKillSwitch Activate(
        Guid? accountId,
        string? activatedBy,
        string? reason)
    {
        return new RiskKillSwitch
        {
            AccountId = accountId,
            ActivatedAt = DateTime.UtcNow,
            ActivatedBy = activatedBy,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Deactivate(string? deactivatedBy)
    {
        if (DeactivatedAt.HasValue)
            throw new InvalidOperationException(
                "Kill switch is already deactivated.");

        DeactivatedAt = DateTime.UtcNow;
        DeactivatedBy = deactivatedBy;
        Touch();
    }

    public bool IsActive => DeactivatedAt is null;
}