using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

public sealed class DynamicCapitalAllocator : ICapitalAllocator
{
    private readonly IAccountCashCache _cashCache;
    private readonly ILogger<DynamicCapitalAllocator> _logger;

    public DynamicCapitalAllocator(
        IAccountCashCache cashCache,
        ILogger<DynamicCapitalAllocator> logger)
    {
        _cashCache = cashCache;
        _logger = logger;
    }

    public async Task<decimal> GetAllocatableCapitalAsync(
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        var allocation = context.GetAllocationFor(accountId).CapitalAllocation;

        if (!allocation.IsDynamic)
            throw new InvalidOperationException(
                $"DynamicCapitalAllocator called for account {accountId} " +
                $"with mode {allocation.Mode}. Expected Dynamic.");

        var cash = await _cashCache.GetOrRefreshAsync(accountId, ct);
        var percent = allocation.PercentOfBuyPower!.Value;
        var allocated = cash.BuyPowerT0 * (percent / 100m);

        _logger.LogDebug(
            "Dynamic capital for (run={RunId}, account={AccountId}): " +
            "BuyPower={BuyPower:N0} × {Percent}% = {Allocated:N0}",
            context.RunId, accountId, cash.BuyPowerT0, percent, allocated);

        return allocated;
    }
}