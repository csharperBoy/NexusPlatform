namespace Trader.Application.Runtime;

/// <summary>درصد تخصیص یک Account در یک Run.</summary>
public sealed record RunAccountAllocation(
    Guid AccountId,
    decimal Percent);