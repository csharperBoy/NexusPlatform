namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// Stage 4: Sync سفارش‌های باز با Broker و اعمال Fill/Cancel روی Positionها.
/// </summary>
public interface IOrderLifecycleStage
{
    Task RunOnceAsync(CancellationToken ct = default);
}