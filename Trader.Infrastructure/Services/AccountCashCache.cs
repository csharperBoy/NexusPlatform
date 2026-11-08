using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

/// <summary>
/// Singleton. Cache from broker. Invalidate-driven.
/// MaxAge = 30 min برای fail-safe (اگر Invalidate فراموش شد).
/// </summary>
public sealed class AccountCashCache : IAccountCashCache
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<Guid, AccountCashSnapshot> _cache = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountCashCache> _logger;

    public AccountCashCache(
        IServiceScopeFactory scopeFactory,
        ILogger<AccountCashCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public AccountCashSnapshot? TryGet(Guid accountId)
        => _cache.TryGetValue(accountId, out var s) ? s : null;

    public async Task<AccountCashSnapshot> GetOrRefreshAsync(
        Guid accountId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(accountId, out var cached)
            && DateTime.UtcNow - cached.RefreshedAt < MaxAge)
        {
            return cached;
        }

        return await RefreshAsync(accountId, ct);
    }

    public async Task<AccountCashSnapshot> RefreshAsync(
        Guid accountId, CancellationToken ct = default)
    {
        var lockObj = _locks.GetOrAdd(accountId, _ => new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync(ct);
        try
        {
            // Double-check: شاید در طول انتظار، Request دیگری refresh کرده
            if (_cache.TryGetValue(accountId, out var existing)
                && DateTime.UtcNow - existing.RefreshedAt < TimeSpan.FromSeconds(2))
            {
                return existing;
            }

            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
            var sessionProvider = scope.ServiceProvider
                .GetRequiredService<IBrokerSessionProvider>();
            var brokerFactory = scope.ServiceProvider
                .GetRequiredService<IBrokerClientFactory>();

            var account = await uow.Accounts.GetByIdAsync(accountId)
                ?? throw new InvalidOperationException($"Account {accountId} not found.");

            var session = await sessionProvider.GetSessionAsync(accountId, ct);
            var reader = brokerFactory.Get<IBrokerAccountReader>(account.Broker);
            var money = await reader.GetCashBalanceAsync(session, ct);

            var snapshot = new AccountCashSnapshot(
                AccountId: accountId,
                BuyPowerT0: money.BuyPowerT0,
                RefreshedAt: DateTime.UtcNow,
                Source: "Broker");

            _cache[accountId] = snapshot;

            _logger.LogDebug(
                "Cash refreshed for account {AccountId}: BuyPower={BuyPower:N0}",
                accountId, money.BuyPowerT0);

            return snapshot;
        }
        finally
        {
            lockObj.Release();
        }
    }

    public void Invalidate(Guid accountId)
    {
        if (_cache.TryRemove(accountId, out _))
            _logger.LogDebug(
                "Cash cache invalidated for account {AccountId}", accountId);
    }
}