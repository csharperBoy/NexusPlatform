using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Infrastructure.Health;
namespace Trader.Infrastructure.Data
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
        public static IServiceCollection Trader_AddHealthChecks(
            this IServiceCollection services,
            IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");

            // ─── Health State (Singleton) ───
            services.AddSingleton<
                Trader.Application.Abstractions.Health.IRuntimeHealthState,
                RuntimeHealthState>();

            // ─── Database (existing platform helper + tags) ───
            services.AddHealthChecks()
                .AddDbContextCheck<TraderDbContext>(
                    name: "TraderDatabase",
                    tags: new[] { "db" })

                // ─── Custom ───
                .AddCheck<RuntimeWorkerHealthCheck>(
                    "RuntimeWorker", tags: new[] { "runtime" })
                .AddCheck<BacktestWorkerHealthCheck>(
                    "BacktestWorker", tags: new[] { "backtest" })
                .AddCheck<HistoricalWorkerHealthCheck>(
                    "HistoricalWorker", tags: new[] { "historical" })
                .AddCheck<BrokerSessionHealthCheck>(
                    "BrokerSession", tags: new[] { "broker" })
                .AddCheck<MarketDataFreshnessHealthCheck>(
                    "MarketDataFreshness", tags: new[] { "marketdata" });

            return services;
        }
    }
}