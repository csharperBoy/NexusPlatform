using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Models
{
    /*
     📌 CorsSettings
     ----------------
     این کلاس مدل تنظیمات CORS (Cross-Origin Resource Sharing) در لایه Application است.
     هدف آن فراهم کردن یک ساختار ساده برای تعریف Originهای مجاز در سیستم می‌باشد.

     ✅ نکات کلیدی:
     - AllowedOrigins → آرایه‌ای از رشته‌ها که شامل لیست دامنه‌هایی است
       که اجازه دسترسی به API را دارند.
       مثال: ["https://example.com", "https://client.app"]

     - مقدار پیش‌فرض:
       → Array.Empty<string>() → یعنی در حالت اولیه هیچ Origin مجاز نیست.
       → این مقدار باید در زمان پیکربندی (Startup/Program.cs) مقداردهی شود.

     🛠 جریان کار:
     1. در فایل تنظیمات (appsettings.json) بخش CorsSettings تعریف می‌شود:
        {
          "CorsSettings": {
            "AllowedOrigins": [ "https://example.com", "https://client.app" ]
          }
        }

     2. این تنظیمات توسط IConfiguration خوانده می‌شود و به CorsSettings بایند می‌شود.
     3. در زمان راه‌اندازی سرویس‌ها، این تنظیمات به Middleware مربوط به CORS داده می‌شود.
     4. سیستم فقط به درخواست‌هایی از Originهای مجاز پاسخ می‌دهد.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **CORS Configuration** در معماری ماژولار است و تضمین می‌کند
     که تنظیمات امنیتی مربوط به دسترسی بین دامنه‌ها به صورت strongly-typed و قابل مدیریت باشند.
    */

    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>(); // 📌 لیست Originهای مجاز
    }
}
