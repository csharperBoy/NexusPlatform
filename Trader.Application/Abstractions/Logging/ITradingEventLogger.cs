using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Logging;

/// <summary>
/// Logger رویدادهای Trading. Buffer می‌کند، فقط با FlushAsync به DB می‌نویسد.
/// Scoped — یک نمونه در هر Tick.
/// </summary>
public interface ITradingEventLogger
{
    /// <summary>
    /// ثبت یک رویداد. اگر Severity کمتر از MinSeverity تنظیم‌شده باشد، Ignore می‌شود.
    /// Thread-safe نیست — فقط از یک scope (تک‌رشته‌ای) استفاده شود.
    /// </summary>
    void Log(
        TradingStage stage,
        TradingEventType type,
        TradingEventSeverity severity,
        string message,
        object? payload = null,
        Guid? runId = null,
        string? isin = null,
        Guid? accountId = null,
        Guid? signalId = null,
        Guid? orderIntentId = null,
        Guid? brokerOrderId = null,
        Guid? tradeId = null,
        Guid? positionId = null);

    /// <summary>
    /// نوشتن همه‌ی Buffer به DB در یک Transaction. باید در پایان هر Tick صدا زده شود.
    /// </summary>
    Task FlushAsync(CancellationToken ct = default);
}