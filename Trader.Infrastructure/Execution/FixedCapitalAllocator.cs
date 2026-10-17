using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

public sealed class FixedCapitalAllocator : ICapitalAllocator
{
    public Task<decimal> GetAllocatableCapitalAsync(
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        if (!context.CapitalAllocation.IsFixed)
            throw new InvalidOperationException(
                "FixedCapitalAllocator called for dynamic allocation.");

        var amount = context.CapitalAllocation.FixedAmount?.Amount ?? 0m;
        return Task.FromResult(amount);
    }
}