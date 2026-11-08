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
        // ── 1. BuyPower از Cache (یا Broker اگر invalidated/miss) ──
        var cash = await _cashCache.GetOrRefreshAsync(accountId, ct);

        // ── 2. درصد این Account در این Run ──
        var percent = context.GetAllocationFor(accountId);

        // ── 3. Allocated ──
        var allocated = cash.BuyPowerT0 * (percent / 100m);

        _logger.LogDebug(
            "Allocated {Allocated:N0} for (run={RunId}, account={AccountId}): " +
            "BuyPower={BuyPower:N0} × {Percent}%",
            allocated, context.RunId, accountId,
            cash.BuyPowerT0, percent);

        return allocated;
    }
}