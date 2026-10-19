namespace Trader.Application.Abstractions.Positions;

/// <summary>
/// بستن Positionهای یک Run یا مجموعه‌ای از Positionها.
/// برای Stop Run با ClosePositions و Market Close Behavior.
/// </summary>
public interface IPositionCloser
{
    /// <summary>
    /// Sell OrderIntent برای همه‌ی Positionهای باز یک Run ارسال می‌کند.
    /// خروجی: تعداد موفق (Dispatched).
    /// </summary>
    Task<int> CloseRunPositionsAsync(
        Guid runId, string reason, CancellationToken ct = default);

    /// <summary>همان، ولی با Positionهای مشخص.</summary>
    Task<int> ClosePositionsAsync(
        IReadOnlyList<Guid> positionIds, string reason,
        CancellationToken ct = default);
}