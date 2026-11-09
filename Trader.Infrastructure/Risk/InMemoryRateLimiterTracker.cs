using System.Collections.Concurrent;
using Trader.Application.Abstractions.Risk;

namespace Trader.Infrastructure.Risk;

/// <summary>
/// Singleton. Sliding window از timestamps per Account.
/// حداکثر اندازه‌ی queue: bounded by (limit + burst) — Trim روی هر Enqueue.
/// </summary>
public sealed class InMemoryRateLimiterTracker : IRateLimiterTracker
{
    // Order of ConcurrentQueue<T>.Enqueue و Count — consistent.
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<DateTime>> _windows = new();

    public int GetCountInWindow(Guid accountId, TimeSpan window)
    {
        if (!_windows.TryGetValue(accountId, out var queue))
            return 0;

        var cutoff = DateTime.UtcNow - window;

        // Read-only — به queue دست نمی‌زنیم. Trim در RecordDispatch انجام می‌شود.
        // برای rate limit ~10/min + burst، اندازه‌ی queue کوچک است.
        var count = 0;
        foreach (var ts in queue)
        {
            if (ts >= cutoff) count++;
        }
        return count;
    }

    public void RecordDispatch(Guid accountId)
    {
        var queue = _windows.GetOrAdd(accountId, _ => new ConcurrentQueue<DateTime>());
        queue.Enqueue(DateTime.UtcNow);

        // Trim entries older than 5× window — bounded memory.
        // Window اصلی در GetCount اجرا می‌شود، این فقط memory safety است.
        var memoryCutoff = DateTime.UtcNow - TimeSpan.FromMinutes(10);
        while (queue.TryPeek(out var oldest) && oldest < memoryCutoff)
            queue.TryDequeue(out _);
    }

    public void Reset(Guid accountId)
    {
        _windows.TryRemove(accountId, out _);
    }
}