using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Domain.Entities;
using Trader.ExternalApiClient;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Brokers;

/// <summary>
/// مدیریت Session فعال برای هر Account.
/// Scoped است چون ISecretProtector و UoW هر دو Scoped هستند.
/// Cache و Lockها در IBrokerSessionCache (Singleton) نگه‌داری می‌شوند.
/// </summary>
public sealed class BrokerSessionProvider : IBrokerSessionProvider
{
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    private readonly IBrokerSessionCache _cache;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly ISecretProtector _protector;
    private readonly ILogger<BrokerSessionProvider> _logger;

    public BrokerSessionProvider(
        IBrokerSessionCache cache,
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        ISecretProtector protector,
        ILogger<BrokerSessionProvider> logger)
    {
        _cache = cache;
        _uow = uow;
        _brokerFactory = brokerFactory;
        _protector = protector;
        _logger = logger;
    }

    public async Task<ExternalSystemSession> GetSessionAsync(
        Guid accountId, CancellationToken ct = default)
    {
        // ─── Fast path: Cache ───
        if (_cache.TryGet(accountId, out var cached) && IsValid(cached!))
            return cached!;

        // ─── Slow path: per-account Lock ───
        using var _ = await _cache.AcquireLockAsync(accountId, ct);

        // Double-check داخل Lock
        if (_cache.TryGet(accountId, out cached) && IsValid(cached!))
            return cached!;

        var account = await _uow.Accounts.GetByIdAsync(accountId)
            ?? throw new InvalidOperationException(
                $"TraderAccount {accountId} not found.");

        var sessionManager = _brokerFactory.Get<IBrokerSessionManager>(account.Broker);

        // ─── تلاش برای Session ذخیره‌شده در DB ───
        if (!string.IsNullOrWhiteSpace(account.EncryptedSession)
            && account.SessionExp.HasValue)
        {
            try
            {
                var json = _protector.Unprotect(account.EncryptedSession);
                var existing = sessionManager.DeserializeSession(json);

                if (IsValid(existing))
                {
                    _cache.Set(accountId, existing);
                    _logger.LogDebug(
                        "Session restored from storage for account {AccountId}",
                        accountId);
                    return existing;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to restore stored session for account {AccountId}; re-login.",
                    accountId);
            }
        }

        // ─── Login مجدد ───
        _logger.LogInformation(
            "Logging in account {AccountId} ({Username}) broker={Broker}",
            accountId, account.Username, account.Broker);

        var password = _protector.Unprotect(account.EncryptedPassword);
        var fresh = await sessionManager.LoginAsync(account.Username, password, ct);

        PersistSession(account, sessionManager, fresh);
        await _uow.Accounts.UpdateAsync(account);
        await _uow.SaveChangesAsync(ct);

        _cache.Set(accountId, fresh);

        _logger.LogInformation(
            "Session acquired for account {AccountId}, expires={Exp}",
            accountId, fresh.ExpiresAt);

        return fresh;
    }

    public void Invalidate(Guid accountId) => _cache.Invalidate(accountId);

    /* ═══════════════════ Helpers ═══════════════════ */

    private void PersistSession(
        TraderAccount account,
        IBrokerSessionManager sessionManager,
        ExternalSystemSession session)
    {
        var json = sessionManager.SerializeSession(session);
        var encrypted = _protector.Protect(json);
        account.SetSession(encrypted, session.ExpiresAt);
    }

    private static bool IsValid(ExternalSystemSession session)
    {
        // اگر ExpiresAt ندارد، فرض می‌کنیم معتبر است (رفتار Brokerهای بدون Expiry)
        if (session.ExpiresAt is null) return true;
        return session.ExpiresAt.Value > DateTimeOffset.UtcNow + ExpiryBuffer;
    }
}