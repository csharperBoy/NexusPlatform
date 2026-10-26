namespace Trader.Domain.Enums;

/// <summary>
/// سطح اهمیت رویداد. برای فیلتر کردن لاگ‌ها.
/// </summary>
public enum TradingEventSeverity
{
    Debug = 0,      // SnapshotUpdated، IndicatorCalculated (خیلی زیاد)
    Info = 1,       // SignalGenerated، OrderDispatched، PositionOpened
    Warning = 2,    // MarketClosed، NoSignal، TradeCanceled
    Error = 3,      // FetchFailed، OrderDispatchFailed
    Critical = 4,   // RunFaulted
}