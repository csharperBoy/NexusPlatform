using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Infrastructure.Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Infrastructure.DependencyInjection
{
    /*
     📌 HealthCheckExtensions
     ------------------------
     این کلاس یک Extension برای IServiceCollection است که وظیفه‌اش اضافه کردن Health Checkها
     به برنامه می‌باشد. Health Checkها برای بررسی وضعیت سرویس‌ها و دیتابیس‌ها استفاده می‌شوند
     و معمولاً در مسیر `/health` یا مشابه آن در API در دسترس قرار می‌گیرند.

     ✅ نکات کلیدی:
     - از AddHealthChecks استفاده می‌کنیم تا سرویس‌های مختلف را مانیتور کنیم.
     - AddDbContextCheck<SampleDbContext> بررسی می‌کند که آیا دیتابیس ماژول Sample در دسترس است یا نه.
     - نام Health Check برای دیتابیس "SampleDatabase" تعیین شده است.
     - Connection String از IConfiguration خوانده می‌شود (هرچند در اینجا فقط گرفته شده و استفاده نشده).

     🛠 جریان کار:
     1. در زمان راه‌اندازی برنامه، متد Sample_AddHealthChecks فراخوانی می‌شود.
     2. Health Checkها به DI Container اضافه می‌شوند.
     3. وقتی کلاینت یا سرویس مانیتورینگ مسیر `/health` را فراخوانی کند:
        - وضعیت دیتابیس SampleDbContext بررسی می‌شود.
        - نتیجه Healthy یا Unhealthy برگردانده می‌شود.
     4. این قابلیت معمولاً با ابزارهایی مثل Kubernetes, Docker, یا Load Balancerها استفاده می‌شود
        تا وضعیت سرویس بررسی شود.

     📌 نتیجه:
     این کلاس تضمین می‌کند که سرویس Sample همیشه قابل مانیتور باشد
     و وضعیت دیتابیس آن به صورت استاندارد در Health Checkها گزارش شود.
    */

    public static class HealthCheckExtensions
    {
        public static IServiceCollection Sample_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            // 📌 گرفتن Connection String (در صورت نیاز برای Health Checkهای سفارشی)
            var conn = config.GetConnectionString("DefaultConnection");

            // 📌 اضافه کردن Health Check برای دیتابیس SampleDbContext
            services.AddHealthChecks()
                    .AddDbContextCheck<SampleDbContext>("SampleDatabase");

            return services;
        }
    }
}
