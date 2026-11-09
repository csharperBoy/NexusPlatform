using Trader.Domain.ValueObjects;

namespace Trader.Application.Runtime;

/// <summary>تخصیص سرمایه‌ی یک Account در یک Run.</summary>
public sealed record RunAccountAllocation(
    Guid AccountId,
    AccountCapitalAllocation CapitalAllocation);