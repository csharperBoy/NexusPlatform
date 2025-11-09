using MediatR;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Behaviors
{
    /*
     📌 RetryBehavior<TRequest, TResponse>
     -------------------------------------
     این کلاس یک **Pipeline Behavior** در MediatR است که وظیفه‌اش اجرای درخواست‌ها
     (Command/Query) با سیاست‌های Retry می‌باشد. این کار باعث افزایش پایداری سیستم
     در برابر خطاهای موقت (Transient Failures) می‌شود.

     ✅ نکات کلیدی:
     - از IPipelineBehavior<TRequest, TResponse> ارث‌بری می‌کند:
       → این الگو به ما اجازه می‌دهد منطق مشترک (Cross-Cutting Concerns) را
         قبل یا بعد از اجرای Handler اضافه کنیم.
       → در اینجا منطق Retry قبل از اجرای Handler اعمال می‌شود.

     - وابستگی:
       → IReadOnlyPolicyRegistry<string> → رجیستری سیاست‌ها (Policy Registry) از کتابخانه Polly.
         این رجیستری شامل سیاست‌های مختلف مثل Retry, Circuit Breaker, Timeout است.
         در اینجا سیاست "DefaultRetry" استفاده می‌شود.

     - متد Handle:
       1. سیاست Retry از رجیستری گرفته می‌شود (با کلید "DefaultRetry").
       2. متد ExecuteAsync از Polly فراخوانی می‌شود تا اجرای Handler درون سیاست Retry قرار گیرد.
       3. اگر Handler خطای موقت (مثل Timeout یا Exception شبکه) بدهد، Polly به صورت خودکار Retry می‌کند.
       4. در نهایت پاسخ موفق یا خطای نهایی برگردانده می‌شود.

     🛠 جریان کار:
     1. کاربر یک Command یا Query ارسال می‌کند.
     2. MediatR آن را به Handler مربوطه می‌فرستد.
     3. RetryBehavior قبل از اجرای Handler سیاست Retry را اعمال می‌کند.
     4. اگر خطای موقت رخ دهد، Polly چند بار تلاش می‌کند.
     5. اگر موفق شود → پاسخ برگردانده می‌شود.
     6. اگر بعد از همه تلاش‌ها شکست بخورد → Exception نهایی برگردانده می‌شود.

     📌 نتیجه:
     این کلاس تضمین می‌کند که درخواست‌ها در برابر خطاهای موقت مقاوم باشند
     و سیستم پایدارتر عمل کند. این کار باعث رعایت اصل **Resilience & Reliability**
     در معماری ماژولار می‌شود.
    */

    public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IReadOnlyPolicyRegistry<string> _policies;

        public RetryBehavior(IReadOnlyPolicyRegistry<string> policies)
        {
            _policies = policies;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            // 📌 گرفتن سیاست Retry از رجیستری
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");

            // 📌 اجرای Handler درون سیاست Retry
            return await policy.ExecuteAsync(async _ => await next(), ct);
        }
    }
}
