using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Logging
{
    /*
     📌 CorrelationIdEnricher
     ------------------------
     این کلاس یک **Serilog LogEvent Enricher** است که وظیفه‌ی افزودن شناسه‌ی رهگیری (CorrelationId)
     به هر لاگ را بر عهده دارد. هدف آن ایجاد قابلیت رهگیری درخواست‌ها در سیستم‌های توزیع‌شده است.

     ✅ نکات کلیدی:
     - AsyncLocal<string?> _asyncCorrelationId:
       • ذخیره‌ی CorrelationId در کانتکست Async برای سناریوهایی که HttpContext وجود ندارد.
       • این کار باعث می‌شود حتی در پردازش‌های غیر HTTP (Background Jobs, Console Apps) هم CorrelationId داشته باشیم.

     - Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory):
       • متد اصلی برای افزودن CorrelationId به لاگ.
       • اگر CorrelationId موجود باشد، به عنوان Property به لاگ اضافه می‌شود.
       • نام Property → "CorrelationId".

     - GetCorrelationId():
       • تلاش برای خواندن CorrelationId از HttpContext:
         1. هدر "X-Correlation-Id" → شناسه‌ی رهگیری سفارشی.
         2. هدر "Request-Id" → شناسه‌ی پیش‌فرض ASP.NET Core.
       • اگر هیچ‌کدام وجود نداشت → استفاده از مقدار AsyncLocal یا تولید Guid جدید.
       • این طراحی تضمین می‌کند که همیشه یک CorrelationId وجود دارد.

     - SetCorrelationId(string id):
       • امکان تنظیم دستی CorrelationId در سناریوهای غیر HTTP.
       • مثال: در Background Worker می‌توان قبل از شروع پردازش، CorrelationId ست کرد.

     🛠 جریان کار:
     1. هر درخواست HTTP یک CorrelationId دارد (از هدر یا تولید جدید).
     2. این کلاس آن شناسه را به لاگ اضافه می‌کند.
     3. در سیستم‌های توزیع‌شده، می‌توان همه‌ی لاگ‌های مربوط به یک درخواست را با CorrelationId رهگیری کرد.
     4. ابزارهای مانیتورینگ و لاگ‌گذاری (Elastic, Seq, Kibana) می‌توانند بر اساس CorrelationId فیلتر کنند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **CorrelationId Logging** در معماری ماژولار است
     و تضمین می‌کند که همه‌ی لاگ‌ها قابل رهگیری و مرتبط با درخواست اصلی باشند.
    */

    public class CorrelationIdEnricher : ILogEventEnricher
    {
        // 📌 Fallback برای سناریوهای غیر HTTP
        private static readonly AsyncLocal<string?> _asyncCorrelationId = new();

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = GetCorrelationId();
            if (!string.IsNullOrEmpty(correlationId))
            {
                var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
                logEvent.AddPropertyIfAbsent(property);
            }
        }

        private static string GetCorrelationId()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext?.Request?.Headers != null)
            {
                if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                    return correlationId!;
                if (httpContext.Request.Headers.TryGetValue("Request-Id", out var requestId))
                    return requestId!;
            }

            return _asyncCorrelationId.Value ?? Guid.NewGuid().ToString();
        }

        // 📌 امکان تنظیم دستی CorrelationId
        public static void SetCorrelationId(string id) => _asyncCorrelationId.Value = id;
    }
}
