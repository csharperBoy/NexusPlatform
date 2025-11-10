using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Logging
{
    /*
     📌 CorrelationIdMiddleware
     --------------------------
     این Middleware وظیفه‌ی مدیریت و تزریق شناسه‌ی رهگیری (CorrelationId) در چرخه‌ی درخواست HTTP را بر عهده دارد.
     هدف آن ایجاد قابلیت **Traceability** در سیستم‌های توزیع‌شده است تا بتوان همه‌ی لاگ‌ها و رخدادهای مربوط به یک درخواست را دنبال کرد.

     ✅ نکات کلیدی:
     - RequestDelegate _next:
       • اشاره به Middleware بعدی در Pipeline.

     - Invoke(HttpContext context):
       • دریافت یا تولید یک CorrelationId برای درخواست جاری.
       • افزودن CorrelationId به Response Header (X-Correlation-Id).
       • ذخیره CorrelationId در AsyncLocal از طریق CorrelationIdEnricher.SetCorrelationId → برای استفاده در Threadهای غیر HTTP.
       • PushProperty در Serilog LogContext → تا همه‌ی لاگ‌های این درخواست شامل CorrelationId باشند.
       • فراخوانی Middleware بعدی در Pipeline.

     - GetOrCreateCorrelationId(HttpContext context):
       • تلاش برای خواندن CorrelationId از Request Header ("X-Correlation-Id").
       • اگر وجود نداشت → تولید یک Guid جدید به عنوان CorrelationId.

     🛠 جریان کار:
     1. هر درخواست وارد اپلیکیشن می‌شود.
     2. اگر هدر "X-Correlation-Id" وجود داشته باشد → همان مقدار استفاده می‌شود.
     3. اگر وجود نداشته باشد → یک Guid جدید تولید می‌شود.
     4. مقدار CorrelationId در Response Header قرار می‌گیرد.
     5. مقدار CorrelationId در LogContext و AsyncLocal ذخیره می‌شود.
     6. همه‌ی لاگ‌ها و رخدادهای مربوط به این درخواست شامل CorrelationId خواهند بود.

     📌 نتیجه:
     این Middleware پایه‌ی مکانیزم **CorrelationId Propagation** در معماری ماژولار است
     و تضمین می‌کند که همه‌ی درخواست‌ها قابل رهگیری باشند و لاگ‌ها به صورت استاندارد به یکدیگر مرتبط شوند.
    */

    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            // 📌 ذخیره در AsyncLocal برای استفاده در Threadهای غیر HTTP
            Core.Infrastructure.Logging.CorrelationIdEnricher.SetCorrelationId(correlationId);

            // 📌 افزودن CorrelationId به LogContext برای Serilog
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                return correlationId!;
            return Guid.NewGuid().ToString();
        }
    }
}
