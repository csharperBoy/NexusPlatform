namespace Trader.Infrastructure.Services.Retention;

public interface ITradingEventRetentionService
{
    /// <summary>
    /// پاک‌سازی رویدادهای قدیمی‌تر از RetentionDays.
    /// خروجی: تعداد رویدادهای حذف‌شده.
    /// </summary>
    Task<int> CleanupAsync(CancellationToken ct = default);
}