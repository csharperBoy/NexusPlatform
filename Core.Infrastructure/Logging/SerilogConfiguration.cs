using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Logging
{
    /*
     📌 SerilogConfiguration
     -----------------------
     این کلاس یک **Helper Static Class** است که وظیفه‌ی ایجاد و پیکربندی Logger اصلی سیستم
     با استفاده از Serilog را بر عهده دارد.

     ✅ نکات کلیدی:
     - CreateConfiguration(IConfiguration configuration):
       • نقطه‌ی ورودی برای ساخت LoggerConfiguration.
       • خواندن تنظیمات از IConfiguration (appsettings.json).
       • افزودن اطلاعات Contextی به لاگ‌ها:
         1. Application → نام اپلیکیشن (از تنظیمات یا مقدار پیش‌فرض).
         2. Environment → محیط اجرا (Development, Staging, Production).
         3. MachineName → نام ماشین اجراکننده.
         4. ThreadId → شناسه‌ی Thread.
         5. LogContext → اطلاعات Contextی اضافه شده در طول اجرای برنامه.
         6. CorrelationIdEnricher → شناسه‌ی رهگیری درخواست‌ها.

       • مقصدهای لاگ‌گذاری (Sinks):
         - Console → نمایش لاگ‌ها در کنسول با قالب سفارشی.
         - Debug → ارسال لاگ‌ها به Debug Output (برای توسعه).

       • سطح لاگ‌ها (MinimumLevel.Override):
         - Microsoft → فقط Warning و بالاتر.
         - Microsoft.EntityFrameworkCore → فقط Warning و بالاتر.
         - System → فقط Warning و بالاتر.
         → این کار باعث کاهش نویز لاگ‌ها از کتابخانه‌های خارجی می‌شود.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، این متد فراخوانی می‌شود:
        Log.Logger = SerilogConfiguration.CreateConfiguration(configuration).CreateLogger();
     2. Logger ساخته شده و در DI ثبت می‌شود.
     3. همه‌ی لاگ‌ها با Contextهای استاندارد (Application, Environment, CorrelationId) enrich می‌شوند.
     4. لاگ‌ها در Console و Debug قابل مشاهده هستند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Centralized Logging Configuration** در معماری ماژولار است
     و تضمین می‌کند که لاگ‌ها همیشه استاندارد، قابل رهگیری و قابل مانیتورینگ باشند.
    */

    public static class SerilogConfiguration
    {
        public static LoggerConfiguration CreateConfiguration(IConfiguration configuration)
        {
            var appName = configuration["Application:Name"] ?? "AkSteelWelfarePlatform";
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Application", appName)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationIdEnricher>()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .WriteTo.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        }
    }
}
