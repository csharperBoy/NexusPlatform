using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

/// <summary>
/// سرمایه قابل استفاده = Cash Balance Account در Broker.
/// با Cache کوتاه‌مدت (۵ ثانیه) برای جلوگیری از فشار به Broker API.
/// </summary>
public sealed class DynamicCapitalAllocator : ICapitalAllocator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(5);

    private sealed record Entry(decimal Balance, DateTime LoadedAt);

    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Entry> _cache = new();
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly ILogger<DynamicCapitalAllocator> _logger;

    public DynamicCapitalAllocator(
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        IBrokerSessionProvider sessionProvider,
        ILogger<DynamicCapitalAllocator> logger)
    {
        _uow = uow;
        _brokerFactory = brokerFactory;
        _sessionProvider = sessionProvider;
        _logger = logger;
    }

    public async Task<decimal> GetAllocatableCapitalAsync(
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue(accountId, out var entry)
            && DateTime.UtcNow - entry.LoadedAt < CacheTtl)
        {
            return entry.Balance;
        }

        var account = await _uow.Accounts.GetByIdAsync(accountId);
        if (account is null)
        {
            _logger.LogWarning("Account {AccountId} not found.", accountId);
            return 0m;
        }

        try
        {
            var session = await _sessionProvider.GetSessionAsync(accountId, ct);
            var reader = _brokerFactory.Get<IBrokerAccountReader>(account.Broker);
            var money = await reader.GetCashBalanceAsync(session, ct);

            var balance = money.Amount;
            _cache[accountId] = new Entry(balance, DateTime.UtcNow);

            _logger.LogDebug(
                "Dynamic capital for account {AccountId}: {Balance}",
                accountId, balance);

            return balance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to fetch cash balance for account {AccountId}.", accountId);
            return 0m;
        }
    }
}