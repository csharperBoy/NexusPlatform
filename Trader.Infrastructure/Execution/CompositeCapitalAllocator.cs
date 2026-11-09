using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

/// <summary>
/// از بین Fixed و Dynamic per-account تصمیم می‌گیرد.
/// (قبلاً per-run بود — حالا per-account.)
/// </summary>
public sealed class CompositeCapitalAllocator : ICapitalAllocator
{
    private readonly FixedCapitalAllocator _fixed;
    private readonly DynamicCapitalAllocator _dynamic;

    public CompositeCapitalAllocator(
        FixedCapitalAllocator @fixed,
        DynamicCapitalAllocator dynamicAllocator)
    {
        _fixed = @fixed;
        _dynamic = dynamicAllocator;
    }

    public Task<decimal> GetAllocatableCapitalAsync(
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        var allocation = context.GetAllocationFor(accountId).CapitalAllocation;

        return allocation.IsFixed
            ? _fixed.GetAllocatableCapitalAsync(context, accountId, ct)
            : _dynamic.GetAllocatableCapitalAsync(context, accountId, ct);
    }
}