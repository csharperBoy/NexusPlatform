namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// محاسبه‌ی سرمایه‌ی «مصرف‌شده»:
/// = ارزش موقعیت‌های باز + حجم باقی‌مانده‌ی سفارشات BUY در راه
/// </summary>
public interface ICommittedCapitalCalculator
{
    /// <summary>سرمایه‌ی مصرف‌شده در کل Run (همه‌ی نمادها) در یک Account.</summary>
    Task<decimal> CalculateForRunAsync(
        Guid runId,
        Guid accountId,
        CancellationToken ct = default);

    /// <summary>سرمایه‌ی مصرف‌شده برای یک Symbol خاص.</summary>
    Task<decimal> CalculateForSymbolAsync(
        Guid runId,
        Guid accountId,
        string isin,
        CancellationToken ct = default);
}

