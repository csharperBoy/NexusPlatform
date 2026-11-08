namespace Trader.Application.Abstractions.Risk;

/// <summary>
/// State مشترک Risk که در Cache نگه داشته می‌شود:
///   • Kill Switch (Global + per-Account) — فعال/غیرفعال
///   • (Phase 15+) Daily Loss Opening Equity per Run
///   • (Phase 17) Consecutive Loss Count per Run
///   • (Phase 18) Rate Limit Order Count per Account/Minute
///
/// Persistence: Kill Switch در DB (survives restart)، بقیه in-memory.
/// Cache: on write invalidate/refresh، on startup load از DB.
/// </summary>
public interface IRiskStateTracker
{
    // ── Kill Switch ──
    bool IsGlobalKillSwitchActive { get; }
    bool IsAccountKillSwitchActive(Guid accountId);
    // به IRiskStateTracker اضافه کن:
    IReadOnlySet<Guid> ActiveAccountKillSwitches { get; }
    Task ActivateKillSwitchAsync(
        Guid? accountId, string? activatedBy, string? reason,
        CancellationToken ct = default);

    Task DeactivateKillSwitchAsync(
        Guid? accountId, string? deactivatedBy,
        CancellationToken ct = default);

    Task RefreshAsync(CancellationToken ct = default);
}