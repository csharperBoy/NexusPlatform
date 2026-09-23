using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Infrastructure.Jobs
{
   
    /// <summary>
    /// انتظار دقیق تا یک لحظه‌ی UTC.
    /// دقت: ~۱-۲ms (شبکه‌ی متصل).
    /// </summary>
    public static class PreciseDelay
    {
        public static async Task UntilAsync(long targetUnixMs, CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var remaining = targetUnixMs - now;

                if (remaining <= 0) return;

                if (remaining > 50)
                {
                    // خواب عادی
                    await Task.Delay((int)Math.Min(remaining - 20, 500), ct);
                }
                else if (remaining > 15)
                {
                    // خواب ریز
                    await Task.Delay(5, ct);
                }
                else
                {
                    // busy-wait برای دقت بالا
                    var spin = new SpinWait();
                    while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < targetUnixMs)
                    {
                        ct.ThrowIfCancellationRequested();
                        spin.SpinOnce();
                    }
                    return;
                }
            }
        }

        public static Task UntilAsync(DateTimeOffset target, CancellationToken ct)
            => UntilAsync(target.ToUnixTimeMilliseconds(), ct);
    }
}
