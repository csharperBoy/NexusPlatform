using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Middlewares
{
    /*
     📌 ExceptionHandlingMiddleware
     ------------------------------
     این Middleware وظیفه‌ی مدیریت خطاهای کنترل‌نشده (Unhandled Exceptions) در چرخه‌ی درخواست HTTP را بر عهده دارد.
     هدف آن جلوگیری از Crash اپلیکیشن و بازگرداندن پاسخ استاندارد به کلاینت است.

     ✅ نکات کلیدی:
     - RequestDelegate _next:
       • اشاره به Middleware بعدی در Pipeline.

     - ILogger<ExceptionHandlingMiddleware> _logger:
       • برای ثبت لاگ خطاها و کمک به مانیتورینگ و Debugging.

     - InvokeAsync(HttpContext context):
       • اجرای Middleware بعدی در Pipeline.
       • اگر Exception رخ دهد:
         1. ثبت خطا با سطح Error در Logger.
         2. تنظیم Response به فرمت JSON با StatusCode = 500.
         3. بازگرداندن یک شیء Problem Details شامل:
            - Title → پیام عمومی خطا.
            - Detail → متن خطا (Message).
            - Status → کد وضعیت HTTP.
            - TraceId → شناسه‌ی رهگیری درخواست (برای Debugging و Traceability).

     🛠 جریان کار:
     1. درخواست وارد اپلیکیشن می‌شود.
     2. Middleware بعدی اجرا می‌شود.
     3. اگر Exception رخ دهد:
        • خطا ثبت می‌شود.
        • پاسخ استاندارد JSON به کلاینت بازگردانده می‌شود.
     4. کلاینت همیشه یک پاسخ قابل پیش‌بینی دریافت می‌کند (نه یک Crash یا HTML Error Page).

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Global Exception Handling** در معماری ماژولار است
     و تضمین می‌کند که خطاها به صورت استاندارد مدیریت شوند و تجربه‌ی کاربری بهبود یابد.
    */

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 📌 ثبت خطا در لاگ
                _logger.LogError(ex, "Unhandled exception occurred");

                // 📌 تنظیم پاسخ استاندارد JSON
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var problem = new
                {
                    Title = "An unexpected error occurred",
                    Detail = ex.Message,
                    Status = context.Response.StatusCode,
                    TraceId = context.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
