using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// محاسبه سرمایه قابل استفاده برای یک Run در یک Account مشخص.
/// Fixed: مقدار ثابت از تنظیمات Run.
/// Dynamic: از Cash Balance Account در Broker.
/// </summary>
public interface ICapitalAllocator
{
    Task<decimal> GetAllocatableCapitalAsync(
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default);
}