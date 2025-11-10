using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Logging
{
    /*
     📌 SerilogLoggingService
     ------------------------
     این کلاس پیاده‌سازی اینترفیس ILoggingService است و وظیفه‌ی مدیریت لاگ‌گذاری
     با استفاده از Serilog و ILogger<T> در ASP.NET Core را بر عهده دارد.

     ✅ نکات کلیدی:
     - وابستگی‌ها:
       • ILogger<SerilogLoggingService> → لاگر تزریق‌شده از DI که توسط Serilog مدیریت می‌شود.

     - متدها:
       • LogInformation → ثبت لاگ‌های اطلاعاتی (Info).
       • LogWarning → ثبت هشدارها (Warning).
       • LogError → ثبت خطاها (Error) همراه با Exception.
       • LogDebug → ثبت لاگ‌های Debug برای توسعه و تست.
       • BeginScope → ایجاد Scope جدید برای گروه‌بندی لاگ‌ها (مثلاً همه‌ی لاگ‌های مربوط به یک درخواست).

     - طراحی:
       • این کلاس یک Wrapper ساده روی ILogger<T> است.
       • هدف: جداسازی وابستگی مستقیم به Serilog از لایه‌های بالاتر.
       • این کار باعث می‌شود در آینده بتوان Logger را تغییر داد بدون تغییر در کدهای مصرف‌کننده.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، Serilog به عنوان Logger اصلی پیکربندی می‌شود.
     2. این کلاس در DI ثبت می‌شود به عنوان ILoggingService.
     3. کلاس‌های دامنه و اپلیکیشن به جای استفاده مستقیم از ILogger<T>، از ILoggingService استفاده می‌کنند.
     4. همه‌ی لاگ‌ها به Serilog ارسال می‌شوند و با Contextهای استاندارد (CorrelationId, Application, Environment) enrich می‌شوند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Logging Abstraction Implementation** در معماری ماژولار است
     و تضمین می‌کند که لاگ‌گذاری به صورت استاندارد، قابل تست و قابل توسعه انجام شود.
    */

    public class SerilogLoggingService : ILoggingService
    {
        private readonly ILogger<SerilogLoggingService> _logger;

        public SerilogLoggingService(ILogger<SerilogLoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void LogError(Exception exception, string message, params object[] args)
            => _logger.LogError(exception, message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public IDisposable BeginScope(string scopeName)
            => _logger.BeginScope(scopeName);
    }
}
