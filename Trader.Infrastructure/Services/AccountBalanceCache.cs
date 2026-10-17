using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Infrastructure.Services;

public sealed class AccountBalanceCache : IAccountBalanceCache
{
    private sealed record Entry(decimal Balance, DateTime LoadedAt, TimeSpan Ttl);

    private readonly ConcurrentDictionary<Guid, Entry> _cache = new();
    private readonly ILogger<AccountBalanceCache> _logger;

    public AccountBalanceCache(ILogger<AccountBalanceCache> logger)
    {
        _logger = logger;
    }

    public bool TryGet(Guid accountId, out decimal balance)
    {
        if (_cache.TryGetValue(accountId, out var entry)
            && DateTime.UtcNow - entry.LoadedAt < entry.Ttl)
        {
            balance = entry.Balance;
            return true;
        }

        balance = 0m;
        return false;
    }

    public void Set(Guid accountId, decimal balance, TimeSpan ttl)
    {
        _cache[accountId] = new Entry(balance, DateTime.UtcNow, ttl);
    }

    public void Invalidate(Guid accountId)
    {
        if (_cache.TryRemove(accountId, out _))
            _logger.LogDebug(
                "Balance cache invalidated for account {AccountId}", accountId);
    }
}