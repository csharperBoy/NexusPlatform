using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Behaviors
{
    /*
     📌 LoggingBehavior<TRequest, TResponse>
     ---------------------------------------
     این کلاس یک **Pipeline Behavior** در MediatR است که وظیفه‌اش ثبت لاگ برای
     همه‌ی Requestها (Command/Query) و پاسخ‌های آن‌ها می‌باشد.

     ✅ نکات کلیدی:
     - از IPipelineBehavior<TRequest, TResponse> ارث‌بری می‌کند:
       → این الگو به ما اجازه می‌دهد منطق مشترک (Cross-Cutting Concerns) را
         قبل و بعد از اجرای Handler اضافه کنیم.
       → در اینجا لاگ‌گذاری قبل و بعد از اجرای Handler انجام می‌شود.

     - وابستگی:
       → ILogger<LoggingBehavior<TRequest, TResponse>> → سرویس لاگ‌گذاری استاندارد .NET.

     - متد Handle:
       1. قبل از اجرای Handler:
          - لاگ ثبت می‌شود که چه Requestی در حال پردازش است و Payload آن چیست.
       2. اجرای Handler اصلی با next().
       3. بعد از اجرای Handler:
          - لاگ ثبت می‌شود که چه Requestی پردازش شد و Response آن چه بود.
       4. در نهایت Response به Caller برگردانده می‌شود.

     🛠 جریان کار:
     1. کاربر یک Command یا Query ارسال می‌کند.
     2. MediatR آن را به Handler مربوطه می‌فرستد.
     3. LoggingBehavior قبل از اجرای Handler یک لاگ ثبت می‌کند.
     4. Handler اجرا می‌شود و پاسخ تولید می‌کند.
     5. LoggingBehavior بعد از اجرای Handler یک لاگ دیگر ثبت می‌کند.
     6. پاسخ اصلی به کاربر برگردانده می‌شود.

     📌 نتیجه:
     این کلاس تضمین می‌کند که همه‌ی درخواست‌ها و پاسخ‌ها به صورت خودکار لاگ شوند،
     بدون اینکه نیاز باشد در هر Handler به صورت دستی کد لاگ‌گذاری نوشته شود.
     این کار باعث رعایت اصل **Cross-Cutting Concerns** و ساده‌سازی Handlerها می‌شود.
    */

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // 📌 لاگ قبل از اجرای Handler
            _logger.LogInformation("Handling {RequestName} with payload {@Request}", typeof(TRequest).Name, request);

            var response = await next();

            // 📌 لاگ بعد از اجرای Handler
            _logger.LogInformation("Handled {RequestName} with response {@Response}", typeof(TRequest).Name, response);

            return response;
        }
    }
}
