namespace Trader.Application.Abstractions.Risk;

/// <summary>
/// Sliding-window rate limiter per Account.
/// Singleton، in-memory. Restart → state reset (acceptable — broker
/// rate limit ultimate backstop).
/// </summary>
public interface IRateLimiterTracker
{
    /// <summary>
    /// تعداد dispatchهای موفق در پنجره‌ی داده‌شده (read-only).
    /// </summary>
    int GetCountInWindow(Guid accountId, TimeSpan window);

    /// <summary>
    /// ثبت یک dispatch موفق. Sync، بدون I/O.
    /// </summary>
    void RecordDispatch(Guid accountId);

    /// <summary>پاک‌کردن state یک Account (Admin/Debug).</summary>
    void Reset(Guid accountId);
}