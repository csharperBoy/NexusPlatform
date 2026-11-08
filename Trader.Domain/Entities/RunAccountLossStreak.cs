using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// وضعیت Loss Streak برای یک (Run, Account).
/// Unique: (RunId, AccountId). یک رکورد به ازای هر جفت.
///
/// Lifecycle:
///   • Position با PnL > 0 بسته شد → ConsecutiveLosses = 0، LastWinAt = now
///   • Position با PnL < 0 بسته شد → ConsecutiveLosses++؛ اگر >= N → Circuit
///   • Circuit فعال + Cooldown تمام → Lazy reset در GetAsync
///   • Reset دستی → صفر کردن
/// </summary>
public class RunAccountLossStreak : BaseEntity, IAuditableEntity
{
    public Guid RunId { get; private set; }
    public Guid AccountId { get; private set; }

    public int ConsecutiveLosses { get; private set; }
    public DateTime? LastLossAt { get; private set; }
    public DateTime? LastWinAt { get; private set; }

    public DateTime? CircuitBrokenAt { get; private set; }
    public DateTime? CooldownExpiresAt { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private RunAccountLossStreak() { } // EF

    public static RunAccountLossStreak Create(Guid runId, Guid accountId)
    {
        if (runId == Guid.Empty)
            throw new ArgumentException("RunId required.", nameof(runId));
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId required.", nameof(accountId));

        return new RunAccountLossStreak
        {
            RunId = runId,
            AccountId = accountId,
            ConsecutiveLosses = 0,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// یک Position بسته شد با PnL مثبت. Counter reset.
    /// </summary>
    public void RecordWin(DateTime when)
    {
        ConsecutiveLosses = 0;
        LastWinAt = when;
        // Circuit (اگر فعال بود) شکسته می‌شود — یک Win بعد از N loss
        // نشانه‌ی بازگشت است.
        CircuitBrokenAt = null;
        CooldownExpiresAt = null;
        Touch();
    }

    /// <summary>
    /// یک Position بسته شد با PnL منفی. Counter++.
    /// اگر به Threshold رسید و Circuit فعال نبود → Trigger.
    /// </summary>
    public void RecordLoss(DateTime when, int threshold, TimeSpan cooldown)
    {
        ConsecutiveLosses++;
        LastLossAt = when;

        if (ConsecutiveLosses >= threshold && !CircuitBrokenAt.HasValue)
        {
            CircuitBrokenAt = when;
            CooldownExpiresAt = when + cooldown;
        }

        Touch();
    }

    /// <summary>
    /// Reset صریح — دستی یا Cooldown expiry.
    /// </summary>
    public void Reset()
    {
        ConsecutiveLosses = 0;
        CircuitBrokenAt = null;
        CooldownExpiresAt = null;
        Touch();
    }
}