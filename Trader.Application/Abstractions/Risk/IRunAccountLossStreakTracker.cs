namespace Trader.Application.Abstractions.Risk;

public sealed record LossStreakState(
    Guid RunId,
    Guid AccountId,
    int ConsecutiveLosses,
    DateTime? LastLossAt,
    DateTime? LastWinAt,
    DateTime? CircuitBrokenAt,
    DateTime? CooldownExpiresAt)
{
    public bool IsCircuitActive(DateTime utcNow) =>
        CircuitBrokenAt.HasValue
        && CooldownExpiresAt.HasValue
        && utcNow < CooldownExpiresAt.Value;
}

public interface IRunAccountLossStreakTracker
{
    /// <summary>
    /// وضعیت فعلی. اگر Cooldown تمام شده باشد، Lazy reset می‌کند.
    /// </summary>
    Task<LossStreakState> GetAsync(
        Guid runId, Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Position که تازه بسته شد (State=Closed) را Notify می‌کند.
    /// PnL > 0 → Win، PnL < 0 → Loss، PnL = 0 → Neutral (no-op).
    /// Paper Mode با ApplyToPaper=false → Skip.
    /// </summary>
    Task OnPositionClosedAsync(
        Trader.Domain.Entities.Position position, CancellationToken ct = default);

    /// <summary>Reset صریح — از API یا Cooldown expiry.</summary>
    Task ResetAsync(
        Guid runId, Guid accountId, string? reason, CancellationToken ct = default);

    /// <summary>Reset همه Streakهای یک Run.</summary>
    Task<int> ResetRunAsync(Guid runId, string? reason, CancellationToken ct = default);

    /// <summary>Reset همه Streakهای یک Account.</summary>
    Task<int> ResetAccountAsync(Guid accountId, string? reason, CancellationToken ct = default);

    /// <summary>همه‌ی Circuitهای فعال.</summary>
    Task<IReadOnlyList<LossStreakState>> GetActiveCircuitsAsync(
        CancellationToken ct = default);
}