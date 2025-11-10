using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Models
{
    /*
     📌 HealthCheckSettings
     ----------------------
     این کلاس مدل تنظیمات مربوط به **Health Checks** در سیستم است.
     هدف آن فراهم کردن یک ساختار strongly-typed برای مدیریت وضعیت سلامت سرویس‌ها
     و پیکربندی نقطه‌ی دسترسی (Endpoint) می‌باشد.

     ✅ نکات کلیدی:
     - Enabled → فعال یا غیرفعال بودن مکانیزم Health Check.
       → پیش‌فرض: true (فعال).

     - Endpoint → مسیر (URL) برای Health Check.
       → پیش‌فرض: "/health".
       → معمولاً توسط Load Balancer یا Kubernetes برای بررسی سلامت سرویس استفاده می‌شود.

     - TimeoutSeconds → مدت زمان انتظار برای پاسخ Health Check.
       → پیش‌فرض: 10 ثانیه.
       → اگر سرویس در این زمان پاسخ ندهد، به عنوان Unhealthy در نظر گرفته می‌شود.

     🛠 جریان کار:
     1. در فایل تنظیمات (appsettings.json) بخش HealthCheckSettings تعریف می‌شود:
        {
          "HealthCheckSettings": {
            "Enabled": true,
            "Endpoint": "/health",
            "TimeoutSeconds": 10
          }
        }

     2. این تنظیمات توسط IConfiguration خوانده شده و به HealthCheckSettings بایند می‌شود.
     3. در زمان راه‌اندازی برنامه (Startup/Program.cs)، این تنظیمات به Middleware مربوط به Health Checks داده می‌شود.
     4. سیستم به صورت دوره‌ای وضعیت سلامت سرویس‌ها را بررسی می‌کند.
     5. Load Balancer یا Orchestrator (مثل Kubernetes) بر اساس این وضعیت تصمیم می‌گیرد
        که سرویس را در گردش درخواست‌ها نگه دارد یا حذف کند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Health Monitoring** در معماری ماژولار است و تضمین می‌کند
     که تنظیمات مربوط به بررسی سلامت سرویس‌ها به صورت strongly-typed و قابل مدیریت باشند.
    */

    public class HealthCheckSettings
    {
        public bool Enabled { get; set; } = true;          // 📌 فعال بودن Health Check
        public string Endpoint { get; set; } = "/health";  // 📌 مسیر Health Check
        public int TimeoutSeconds { get; set; } = 10;      // 📌 زمان انتظار برای پاسخ
    }
}
