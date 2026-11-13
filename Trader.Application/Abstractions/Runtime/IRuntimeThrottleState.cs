namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// Throttle state مشترک برای Stageهای runtime (Singleton).
///
/// چرا Singleton؟ Stages با AddScoped ثبت شده‌اند — instance در هر tick عوض
/// می‌شود. State نگه‌داری‌شده در instance (مثل _lastRunAt) در tick بعدی reset
/// می‌شود و throttle کاملاً بی‌اثر می‌گردد.
///
/// این interface فقط throttle OrderLifecycleStage را در اختیار می‌گذارد؛
/// اگر Stage دیگری نیاز به throttle پیدا کرد، یک متد جدا اضافه می‌شود
/// (نه یک generic open-ended TryClaim).
/// </summary>
public interface IRuntimeThrottleState
{
    DateTime LastOrderLifecycleRunAt { get; }

    /// <summary>
    /// Atomic: اگر elapsed از آخرین اجرا >= minInterval بود، claim می‌کند
    /// و true برمی‌گرداند. اگر نه، false.
    /// Thread-safe: در محیط concurrent فقط یک caller true می‌گیرد.
    /// </summary>
    bool TryClaimOrderLifecycle(TimeSpan minInterval);
}