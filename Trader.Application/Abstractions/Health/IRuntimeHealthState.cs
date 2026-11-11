namespace Trader.Application.Abstractions.Health;

/// <summary>
/// وضعیت سلامت Runtime — Singleton، توسط Workerها پر می‌شود، توسط HealthCheckها خوانده می‌شود.
/// Thread-safe (Interlocked).
/// </summary>
public interface IRuntimeHealthState
{
    DateTime? LastTickAt { get; }
    DateTime? LastBacktestPollAt { get; }

    void MarkTickNow();
    void MarkBacktestPollNow();
}