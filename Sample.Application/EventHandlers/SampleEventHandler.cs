using Core.Application.Common.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Events;

namespace Sample.Application.EventHandlers
{
    /*
     📌 SampleEventHandler (بازنویسی شده با DomainEventHandler)
     ---------------------------------------------------------
     این کلاس یک Domain Event Handler است که از کلاس پایه‌ی 
     `DomainEventHandler<TEvent>` در Core ارث‌بری می‌کند.

     ✅ چرا این تغییر؟
     - قبلاً از INotificationHandler<SampleActionEvent> استفاده می‌کردیم.
     - حالا با ارث‌بری از DomainEventHandler<SampleActionEvent>:
       1. یک قرارداد معنایی مشخص داریم که این کلاس فقط برای Domain Eventهاست.
       2. لاگ استاندارد قبل و بعد از هندل کردن رویداد به صورت خودکار انجام می‌شود.
       3. مدیریت خطا به صورت مشترک در Base Class انجام می‌شود.
       4. توسعه‌دهنده فقط روی منطق اصلی تمرکز می‌کند (متد HandleEventAsync).

     🛠 جریان کار:
     1. موجودیت SampleEntity متد MarkSample را صدا می‌زند → رویداد SampleActionEvent تولید می‌شود.
     2. این رویداد توسط Outbox ذخیره و سپس منتشر می‌شود.
     3. این Handler آن را دریافت می‌کند.
     4. عملیات مورد نظر (فراخوانی SampleApiMethodAsync) اجرا می‌شود.
     5. اگر خطا رخ دهد، Polly با سیاست DefaultRetry دوباره تلاش می‌کند.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید Domain Eventها را هندل کنیم،
     چطور از Base Class مشترک برای استانداردسازی استفاده کنیم،
     و چطور قابلیت Resilience (تحمل خطا) را با Polly اضافه کنیم.
    */

    public class SampleEventHandler : DomainEventHandler<SampleActionEvent>
    {
        private readonly ISampleService _sampleService;
        private readonly IReadOnlyPolicyRegistry<string> _policies;

        public SampleEventHandler(
            ISampleService sampleService,
            ILogger<DomainEventHandler<SampleActionEvent>> logger,
            IReadOnlyPolicyRegistry<string> policies)
            : base(logger) // لاگ استاندارد از کلاس پایه
        {
            _sampleService = sampleService;
            _policies = policies;
        }

        // فقط منطق اصلی هندل کردن رویداد اینجا نوشته می‌شود
        protected override async Task HandleEventAsync(SampleActionEvent sample, CancellationToken cancellationToken)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _sampleService.SampleApiMethodAsync(new SampleApiRequest(sample.ActionProperty1, ""));
            }, cancellationToken);
        }
    }
}