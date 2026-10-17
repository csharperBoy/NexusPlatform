using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

public sealed class DynamicCapitalAllocator : ICapitalAllocator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(5);

    private readonly IAccountBalanceCache _cache;
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly ILogger<DynamicCapitalAllocator> _logger;

    public DynamicCapitalAllocator(
        IAccountBalanceCache cache,
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        IBrokerSessionProvider sessionProvider,
        ILogger<DynamicCapitalAllocator> logger)
    {
        _cache = cache;
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
        if (_cache.TryGet(accountId, out var cached))
            return cached;

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

            var balance = (decimal)money.BuyPowerT0;

            _cache.Set(accountId, balance, CacheTtl);

            _logger.LogInformation(
                "Dynamic capital for account {AccountId}: BuyPowerT0={BuyPower} " +
                "(T0={T0}, Block={Block}, Credit={Credit})",
                accountId, balance, money.T0, money.Block, money.Credit);

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