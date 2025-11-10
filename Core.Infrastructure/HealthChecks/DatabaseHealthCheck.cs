using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace Core.Infrastructure.HealthChecks
{
    /*
     📌 DatabaseHealthCheck<TDbContext>
     ----------------------------------
     این کلاس یک **Custom Health Check** در معماری زیرساخت است که وظیفه‌ی بررسی
     سلامت اتصال به دیتابیس را بر عهده دارد.

     ✅ نکات کلیدی:
     - Generic Class:
       • TDbContext → نوع DbContext که باید بررسی شود.
       • این طراحی انعطاف‌پذیری ایجاد می‌کند تا بتوان از آن برای هر دیتابیس استفاده کرد.

     - سازنده:
       • DbContext به صورت Dependency Injection تزریق می‌شود.
       • این کار باعث می‌شود HealthCheck مستقل از نوع دیتابیس باشد.

     - CheckHealthAsync:
       • Stopwatch → برای اندازه‌گیری زمان پاسخ دیتابیس.
       • Database.CanConnectAsync → بررسی می‌کند که آیا اتصال به دیتابیس برقرار می‌شود یا خیر.
       • اگر اتصال موفق باشد → HealthCheckResult.Healthy همراه با زمان پاسخ.
       • اگر اتصال برقرار نشود → HealthCheckResult.Unhealthy.
       • اگر Exception رخ دهد → HealthCheckResult.Unhealthy همراه با پیام خطا و Exception.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، این HealthCheck در DI ثبت می‌شود.
     2. ابزارهای مانیتورینگ (مثل Kubernetes, Prometheus, Azure Monitor) می‌توانند endpoint `/health` را فراخوانی کنند.
     3. این کلاس بررسی می‌کند که آیا دیتابیس در دسترس است یا خیر.
     4. نتیجه به صورت Healthy یا Unhealthy برگردانده می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Database Connectivity Health Check** در معماری ماژولار است
     و تضمین می‌کند که وضعیت دیتابیس همیشه قابل بررسی و مانیتورینگ باشد.
    */

    public class DatabaseHealthCheck<TDbContext> : IHealthCheck
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public DatabaseHealthCheck(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                sw.Stop();

                if (canConnect)
                {
                    return HealthCheckResult.Healthy(
                        $"Database connection successful (ResponseTime: {sw.ElapsedMilliseconds} ms)");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Database connection failed");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database error: {ex.Message}", ex);
            }
        }
    }
}
