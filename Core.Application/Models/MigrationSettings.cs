using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Models
{
    /*
     📌 MigrationSettings
     --------------------
     این کلاس مدل تنظیمات مربوط به **Database Migrations** در سیستم است.
     هدف آن فراهم کردن یک ساختار strongly-typed برای مدیریت استراتژی مهاجرت دیتابیس‌ها
     و کنترل رفتار سیستم در محیط‌های مختلف (Development, Production) می‌باشد.

     ✅ نکات کلیدی:
     - Strategy → استراتژی مهاجرت دیتابیس:
       • Resilient → اجرای مهاجرت‌ها با Retry و مدیریت خطاهای موقت.
       • Force → اجرای مهاجرت‌ها به صورت اجباری حتی در صورت خطا.
       • Skip → عدم اجرای مهاجرت‌ها (برای محیط‌هایی مثل Production حساس).
       • HealthAware → اجرای مهاجرت‌ها فقط در صورتی که سرویس‌ها سالم باشند.

     - MaxRetryCount → تعداد دفعات تلاش مجدد در صورت خطا (پیش‌فرض: 3).
     - RetryDelaySeconds → فاصله زمانی بین تلاش‌های مجدد (پیش‌فرض: 2 ثانیه).
     - EnableHealthChecks → فعال بودن بررسی سلامت سرویس‌ها قبل از مهاجرت.
     - SafeModeInProduction → حالت ایمن در محیط Production (جلوگیری از تغییرات خطرناک).
     - CriticalDbContexts → لیست DbContextهای حیاتی که مهاجرت آن‌ها باید با دقت بیشتری انجام شود
       (مثلاً AuthDbContext یا UserManagementDbContext).

     🛠 جریان کار:
     1. در فایل تنظیمات (appsettings.json) بخش MigrationSettings تعریف می‌شود:
        {
          "MigrationSettings": {
            "Strategy": "Resilient",
            "MaxRetryCount": 3,
            "RetryDelaySeconds": 2,
            "EnableHealthChecks": true,
            "SafeModeInProduction": true,
            "CriticalDbContexts": [ "AuthDbContext", "UserManagementDbContext" ]
          }
        }

     2. این تنظیمات توسط IConfiguration خوانده شده و به MigrationSettings بایند می‌شود.
     3. در زمان راه‌اندازی برنامه، سرویس MigrationManager از این تنظیمات استفاده می‌کند
        تا تصمیم بگیرد مهاجرت‌ها چگونه اجرا شوند.
     4. در محیط Production معمولاً SafeMode فعال است تا از تغییرات خطرناک جلوگیری شود.
     5. در محیط Development یا Test می‌توان از Force یا Skip استفاده کرد.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Database Migration Strategy** در معماری ماژولار است
     و تضمین می‌کند که مهاجرت دیتابیس‌ها به صورت قابل مدیریت، مقاوم در برابر خطا،
     و متناسب با محیط اجرا شوند.
    */

    public class MigrationSettings
    {
        public string Strategy { get; set; } = "Resilient"; // 📌 استراتژی مهاجرت دیتابیس
        public int MaxRetryCount { get; set; } = 3;         // 📌 تعداد دفعات تلاش مجدد
        public int RetryDelaySeconds { get; set; } = 2;     // 📌 فاصله زمانی بین تلاش‌ها
        public bool EnableHealthChecks { get; set; } = true; // 📌 بررسی سلامت سرویس‌ها قبل از مهاجرت
        public bool SafeModeInProduction { get; set; } = true; // 📌 حالت ایمن در محیط Production
        public string[] CriticalDbContexts { get; set; } = new[] { "AuthDbContext", "UserManagementDbContext" }; // 📌 لیست DbContextهای حیاتی
    }
}
