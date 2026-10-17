using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Execution;

/// <summary>
/// بر اساس Mode از Run، به Allocator مناسب Delegate می‌کند.
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
        return context.CapitalAllocation.IsFixed
            ? _fixed.GetAllocatableCapitalAsync(context, accountId, ct)
            : _dynamic.GetAllocatableCapitalAsync(context, accountId, ct);
    }
}