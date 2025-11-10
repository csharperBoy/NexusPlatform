using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Middlewares
{
    /*
     📌 ExceptionHandlingMiddlewareExtensions
     ----------------------------------------
     این کلاس یک **Extension Method** برای IApplicationBuilder فراهم می‌کند
     تا Middleware مدیریت خطا (ExceptionHandlingMiddleware) به راحتی در Pipeline اپلیکیشن ثبت شود.

     ✅ نکات کلیدی:
     - UseExceptionHandling:
       • متدی برای افزودن ExceptionHandlingMiddleware به چرخه‌ی درخواست.
       • با فراخوانی app.UseExceptionHandling() در Program.cs یا Startup.cs،
         Middleware مدیریت خطا فعال می‌شود.
       • این طراحی باعث خوانایی بیشتر و جداسازی مسئولیت‌ها می‌شود.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، توسعه‌دهنده کافی است این Extension را فراخوانی کند:
        app.UseExceptionHandling();
     2. این متد در واقع app.UseMiddleware<ExceptionHandlingMiddleware>() را اجرا می‌کند.
     3. همه‌ی خطاهای کنترل‌نشده در سطح اپلیکیشن توسط ExceptionHandlingMiddleware مدیریت می‌شوند.
     4. پاسخ استاندارد JSON به کلاینت بازگردانده می‌شود و خطاها در لاگ ثبت می‌شوند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Middleware Registration Helper** در معماری ماژولار است
     و تضمین می‌کند که ثبت Middleware مدیریت خطا ساده، خوانا و قابل استفاده مجدد باشد.
    */

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
