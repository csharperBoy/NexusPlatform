using Trader.Application.Abstractions.Runtime;

namespace Trader.Infrastructure.Runtime;

/// <summary>
/// Singleton، Thread-safe، بدون lock. از Interlocked.CompareExchange روی
/// ticks استفاده می‌کند.
///
/// اگر دو thread هم‌زمان TryClaim بزنند: هر دو last را یکسان می‌خوانند،
/// هر دو elapsed check را پاس می‌کنند، ولی CompareExchange فقط برای یکی
/// موفق می‌شود — دومی false می‌گیرد. State نمی‌شکند.
/// </summary>
public sealed class RuntimeThrottleState : IRuntimeThrottleState
{
    private long _lastOrderLifecycleTicks;

    public DateTime LastOrderLifecycleRunAt
    {
        get
        {
            var t = Interlocked.Read(ref _lastOrderLifecycleTicks);
            return t > 0 ? new DateTime(t, DateTimeKind.Utc) : DateTime.MinValue;
        }
    }

    public bool TryClaimOrderLifecycle(TimeSpan minInterval)
    {
        var nowTicks = DateTime.UtcNow.Ticks;

        while (true)
        {
            var lastTicks = Interlocked.Read(ref _lastOrderLifecycleTicks);
            var elapsed = TimeSpan.FromTicks(nowTicks - lastTicks);

            if (lastTicks > 0 && elapsed < minInterval)
                return false;

            // Atomic claim: if another thread already moved _last, retry.
            if (Interlocked.CompareExchange(
                    ref _lastOrderLifecycleTicks, nowTicks, lastTicks) == lastTicks)
                return true;
        }
    }
}