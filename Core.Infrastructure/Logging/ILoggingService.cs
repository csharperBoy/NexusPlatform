using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Logging
{
    /*
     📌 ILoggingService
     ------------------
     این اینترفیس یک abstraction برای سرویس لاگ‌گذاری در سیستم است.
     هدف آن جداسازی منطق لاگ‌گذاری از پیاده‌سازی‌های خاص (مثل Serilog, NLog, ConsoleLogger)
     و ایجاد یک API استاندارد برای ثبت لاگ‌ها در کل اپلیکیشن می‌باشد.

     ✅ نکات کلیدی:
     - LogInformation:
       • ثبت پیام‌های اطلاعاتی (Info).
       • برای رخدادهای عمومی مثل شروع عملیات، وضعیت سیستم، یا موفقیت‌های عادی.

     - LogWarning:
       • ثبت پیام‌های هشدار (Warning).
       • برای شرایط غیرعادی که نیاز به توجه دارند ولی بحرانی نیستند.

     - LogError:
       • ثبت پیام‌های خطا (Error).
       • شامل Exception و پیام توضیحی.
       • برای شرایط بحرانی یا شکست عملیات.

     - LogDebug:
       • ثبت پیام‌های Debug.
       • برای اطلاعات جزئی‌تر که معمولاً در محیط توسعه یا تست استفاده می‌شوند.

     - BeginScope:
       • ایجاد یک Scope جدید برای لاگ‌گذاری.
       • این کار امکان گروه‌بندی لاگ‌ها را فراهم می‌کند (مثلاً همه‌ی لاگ‌های مربوط به یک درخواست یا تراکنش).
       • خروجی IDisposable → پس از پایان Scope باید Dispose شود.

     🛠 جریان کار:
     1. کلاس‌های مختلف در سیستم به جای استفاده مستقیم از Logger، از ILoggingService استفاده می‌کنند.
     2. پیاده‌سازی این اینترفیس (مثلاً LoggingService با Serilog) در DI ثبت می‌شود.
     3. در زمان اجرا، لاگ‌ها به مقصد مناسب (Console, File, Elastic, Seq) ارسال می‌شوند.
     4. این طراحی باعث می‌شود تغییر Logger در آینده بدون تغییر در کدهای مصرف‌کننده امکان‌پذیر باشد.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Logging Abstraction** در معماری ماژولار است
     و تضمین می‌کند که لاگ‌گذاری به صورت استاندارد، قابل تست و قابل توسعه انجام شود.
    */

    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogDebug(string message, params object[] args);
        IDisposable BeginScope(string scopeName);
    }
}
