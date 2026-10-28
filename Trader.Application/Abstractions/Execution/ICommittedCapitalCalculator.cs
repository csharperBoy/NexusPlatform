namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// محاسبه‌ی سرمایه‌ی «در حال استفاده» (Commit شده).
/// = مجموع OrderIntentهای فعال + ارزش موقعیت‌های باز
/// </summary>
public interface ICommittedCapitalCalculator
{
    Task<decimal> CalculateAsync(
        Guid runId,
        Guid accountId,
        CancellationToken ct = default);
}