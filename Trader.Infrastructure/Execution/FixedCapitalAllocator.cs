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
        var allocation = context.GetAllocationFor(accountId).CapitalAllocation;

        if (!allocation.IsFixed)
            throw new InvalidOperationException(
                $"FixedCapitalAllocator called for account {accountId} " +
                $"with mode {allocation.Mode}. Expected Fixed.");

        return Task.FromResult(allocation.FixedAmount!.Amount);
    }
}