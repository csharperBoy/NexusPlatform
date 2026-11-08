namespace Trader.Application.Abstractions.Risk;

/// <summary>Snapshot از Opening Equity برای یک Run در یک روز.</summary>
public sealed record RunDailyEquityOpening(
    Guid RunId,
    DateOnly TradingDate,
    decimal OpeningCash,
    decimal OpeningPositionValue,
    decimal OpeningEquity,
    DateTime CapturedAt);

/// <summary>وضعیت لحظه‌ای Run equity.</summary>
public sealed record RunDailyEquityCurrent(
    RunDailyEquityOpening Opening,
    decimal CurrentCash,
    decimal CurrentPositionValue,
    DateTime LastCashRefreshAt)
{
    public decimal CurrentEquity => CurrentCash + CurrentPositionValue;

    /// <summary>درصد ضرر. اگر سود باشد، 0 (نه منفی).</summary>
    public decimal LossPercent =>
        Opening.OpeningEquity > 0m
            ? Math.Max(0m,
                (Opening.OpeningEquity - CurrentEquity) / Opening.OpeningEquity * 100m)
            : 0m;
}

public interface IRunDailyEquityTracker
{
    /// <summary>
    /// Opening را برای Run در روز داده‌شده برمی‌گرداند.
    /// اگر در DB نبود، از Broker + DB capture می‌کند (یک‌بار per Run per Day).
    /// </summary>
    Task<RunDailyEquityOpening> GetOrCaptureOpeningAsync(
        Guid runId, CancellationToken ct = default);

    /// <summary>
    /// وضعیت لحظه‌ای. اگر از آخرین Refresh بیشتر از BrokerRefreshInterval گذشته،
    /// Cash را از Broker re-fetch می‌کند (با lock per Run).
    /// </summary>
    Task<RunDailyEquityCurrent> GetCurrentAsync(
        Guid runId, CancellationToken ct = default);
}