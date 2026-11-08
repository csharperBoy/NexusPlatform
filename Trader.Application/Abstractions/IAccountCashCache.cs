namespace Trader.Application.Abstractions;

public sealed record AccountCashSnapshot(
    Guid AccountId,
    decimal BuyPowerT0,
    DateTime RefreshedAt,
    string Source);

/// <summary>
/// Cache per-Account از BuyPower.
/// طراحی: Singleton، Invalidate-محور (نه Refresh فوری).
/// - Dispatch موفق → Invalidate
/// - Trade Fill → Invalidate
/// - GetOrRefreshAsync → اگر cache miss یا stale (MaxAge) refresh
/// </summary>
public interface IAccountCashCache
{
    /// <summary>آخرین Snapshot (ممکن است null باشد اگر هنوز گرفته نشده).</summary>
    AccountCashSnapshot? TryGet(Guid accountId);

    /// <summary>
    /// از Broker می‌خواند. همیشه هزینه دارد (~1s).
    /// اگر چند Request هم‌زمان، per-account lock.
    /// </summary>
    Task<AccountCashSnapshot> RefreshAsync(
        Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// اگر Cache miss یا RefreshedAt > MaxAge → Refresh. وگرنه Cache.
    /// بعد از Invalidate، همیشه Refresh می‌کند (cache miss).
    /// </summary>
    Task<AccountCashSnapshot> GetOrRefreshAsync(
        Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// بعد از Dispatch موفق یا Fill — Cash تغییر کرده.
    /// Cache را پاک می‌کند تا next GetOrRefreshAsync از Broker بخواند.
    /// </summary>
    void Invalidate(Guid accountId);
}