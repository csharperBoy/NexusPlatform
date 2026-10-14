using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers;

/// <summary>
/// مدیریت Session فعال برای هر Account.
/// Thread-safe، کش‌شده در حافظه، با Login خودکار هنگام انقضا.
/// </summary>
public interface IBrokerSessionProvider
{
    /// <summary>
    /// Session معتبر برای Account. اگر منقضی باشد، Login می‌کند.
    /// اگر Session موجود و معتبر باشد، از Cache برمی‌گرداند.
    /// </summary>
    Task<ExternalSystemSession> GetSessionAsync(
        Guid accountId, CancellationToken ct = default);

    /// <summary>Session را باطل می‌کند (برای Logout یا خطای احراز هویت).</summary>
    void Invalidate(Guid accountId);
}