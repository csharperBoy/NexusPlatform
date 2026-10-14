using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers;

/// <summary>
/// ذخیره‌گاه Sessionهای فعال. Singleton، Thread-safe.
/// هدف: اشتراک Session بین همه‌ی Scopeها و (در آینده) بین Instanceها.
/// </summary>
public interface IBrokerSessionCache
{
    bool TryGet(Guid accountId, out ExternalSystemSession? session);
    void Set(Guid accountId, ExternalSystemSession session);
    void Invalidate(Guid accountId);

    /// <summary>
    /// Lock per-account برای جلوگیری از Login موازی.
    /// </summary>
    Task<IDisposable> AcquireLockAsync(Guid accountId, CancellationToken ct = default);
}