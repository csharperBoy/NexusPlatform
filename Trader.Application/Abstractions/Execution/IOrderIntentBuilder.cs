using Trader.Application.Runtime;
using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// از Signal + Run Context + Account، یک OrderIntent می‌سازد.
/// خروجی null یعنی Signal در این Account رد شد (مثلاً Capital کافی نیست).
/// </summary>
public interface IOrderIntentBuilder
{
    Task<OrderIntent?> BuildAsync(
        Signal signal,
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default);
}