using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.DependencyInjection
{
    /*
     📌 HealthCheckExtensions
     ------------------------
     این کلاس مجموعه‌ای از **Extension Methods** برای IServiceCollection است
     که وظیفه‌ی ثبت و پیکربندی **Health Checks** در اپلیکیشن را بر عهده دارد.

     ✅ نکات کلیدی:
     - Core_AddHealthChecks:
       • نقطه‌ی ورودی برای افزودن Health Checks به DI Container.
       • در حال حاضر فقط services را برمی‌گرداند (Placeholder).
       • در آینده می‌تواند شامل موارد زیر باشد:
         1. بررسی اتصال به دیتابیس (SQL Server, PostgreSQL, MongoDB).
         2. بررسی دسترسی به سرویس‌های خارجی (APIها، Message Broker).
         3. بررسی وضعیت حافظه، CPU یا سایر منابع سیستم.
         4. ثبت Health Checks سفارشی برای دامنه‌ی اپلیکیشن.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن (Program.cs یا Startup.cs)، این متد فراخوانی می‌شود:
        services.Core_AddHealthChecks(Configuration);
     2. همه‌ی Health Checks مورد نیاز در DI ثبت می‌شوند.
     3. اپلیکیشن می‌تواند از endpoint استاندارد `/health` یا `/healthz` برای بررسی وضعیت استفاده کند.
     4. ابزارهای مانیتورینگ (مثل Kubernetes, Prometheus, Azure Monitor) می‌توانند این endpoint را مصرف کنند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Health Monitoring** در معماری ماژولار است
     و تضمین می‌کند که سرویس‌ها همیشه قابل بررسی و مانیتورینگ باشند.
    */

    public static class HealthCheckExtensions
    {
        public static IServiceCollection Core_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            // 📌 در حال حاضر فقط services را برمی‌گرداند
            // در آینده می‌توان Health Checks مختلف را اینجا ثبت کرد
            // مثال:
            // services.AddHealthChecks()
            //     .AddSqlServer(config.GetConnectionString("DefaultConnection"))
            //     .AddRedis(config.GetConnectionString("Redis"));

            return services;
        }
    }
}
