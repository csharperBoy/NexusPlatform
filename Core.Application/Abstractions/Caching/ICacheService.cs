using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Caching
{
    /*
     📌 ICacheService
     ----------------
     این اینترفیس قرارداد سرویس کش (Caching) در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای مدیریت داده‌های کش شده است،
     بدون وابستگی به جزئیات پیاده‌سازی (مثلاً MemoryCache، Redis، یا Distributed Cache).

     ✅ نکات کلیدی:
     - متدها:
       1. GetAsync<T>(string key)
          → دریافت داده از کش بر اساس کلید.
          → اگر داده وجود نداشته باشد، مقدار null برمی‌گرداند.

       2. SetAsync<T>(string key, T value, TimeSpan? expiration = null)
          → ذخیره داده در کش با کلید مشخص.
          → امکان تعیین زمان انقضا (Expiration) وجود دارد.

       3. RemoveAsync(string key)
          → حذف داده از کش بر اساس کلید.

       4. ExistsAsync(string key)
          → بررسی وجود داده در کش.

       5. RemoveByPatternAsync(string pattern)
          → حذف داده‌ها بر اساس الگوی کلید (Pattern Matching).
          → برای سناریوهایی مثل invalidation بعد از عملیات نوشتن استفاده می‌شود.

     🛠 جریان کار:
     1. سرویس‌های Query داده‌ها را از Repository می‌خوانند.
     2. داده‌ها در کش ذخیره می‌شوند تا درخواست‌های بعدی سریع‌تر پاسخ داده شوند.
     3. سرویس‌های Command بعد از تغییر داده‌ها، کش مرتبط را پاک می‌کنند (Invalidate).
     4. اینترفیس ICacheService تضمین می‌کند که لایه Application فقط قرارداد را بشناسد،
        و پیاده‌سازی در لایه Infrastructure انجام شود.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم کش در معماری ماژولار است و امکان جایگزینی
     پیاده‌سازی‌های مختلف (MemoryCache، Redis، NCache، ...) را بدون تغییر در لایه Application فراهم می‌کند.
    */

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key); // 📌 دریافت داده از کش
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null); // 📌 ذخیره داده در کش با زمان انقضا
        Task RemoveAsync(string key); // 📌 حذف داده از کش
        Task<bool> ExistsAsync(string key); // 📌 بررسی وجود داده در کش
        Task RemoveByPatternAsync(string pattern); // 📌 حذف داده‌ها بر اساس الگو (برای invalidation)
    }
}
