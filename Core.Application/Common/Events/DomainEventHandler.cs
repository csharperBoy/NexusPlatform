using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Common.Events
{
    /*
     📌 DomainEventHandler<TEvent>
     -----------------------------
     این کلاس یک **Base Class** برای همه‌ی EventHandlerهای دامنه است.
     هدف آن فراهم کردن یک اسکلت استاندارد برای پردازش رویدادهای دامنه (Domain Events)
     همراه با لاگ‌گذاری و مدیریت خطا می‌باشد.

     ✅ نکات کلیدی:
     - از IEventHandler<TEvent> ارث‌بری می‌کند:
       → هر رویداد دامنه یک Handler اختصاصی دارد.
       → این کلاس پایه، منطق مشترک همه Handlerها را فراهم می‌کند.

     - Generic Constraint:
       → TEvent : IDomainEvent → فقط رویدادهایی که از IDomainEvent ارث‌بری کرده‌اند قابل پردازش هستند.

     - وابستگی:
       → ILogger<DomainEventHandler<TEvent>> → سرویس لاگ‌گذاری استاندارد .NET.

     - متدها:
       1. HandleAsync(TEvent @event, CancellationToken cancellationToken)
          → متد اصلی برای پردازش رویداد.
          → شامل:
            - لاگ شروع پردازش رویداد.
            - فراخوانی متد انتزاعی HandleEventAsync برای منطق اختصاصی.
            - لاگ موفقیت پردازش.
            - مدیریت خطا و ثبت لاگ در صورت Exception.

       2. HandleEventAsync(TEvent @event, CancellationToken cancellationToken)
          → متد انتزاعی که باید در کلاس‌های فرزند پیاده‌سازی شود.
          → منطق اختصاصی پردازش رویداد در این متد قرار می‌گیرد.

     🛠 جریان کار:
     1. یک رویداد دامنه تولید می‌شود (مثلاً SampleCreatedEvent).
     2. EventBus آن را به Handler مربوطه ارسال می‌کند.
     3. DomainEventHandler وارد عمل می‌شود:
        - لاگ شروع پردازش ثبت می‌شود.
        - متد HandleEventAsync اجرا می‌شود (منطق اختصاصی).
        - لاگ موفقیت یا خطا ثبت می‌شود.
     4. در صورت خطا، Exception دوباره پرتاب می‌شود تا سیستم بتواند آن را مدیریت کند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Domain Event Handling** را استانداردسازی می‌کند،
     و تضمین می‌کند که همه‌ی Handlerها دارای لاگ‌گذاری و مدیریت خطا باشند،
     بدون اینکه نیاز باشد این منطق در هر Handler تکرار شود.
    */

    public abstract class DomainEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        protected readonly ILogger<DomainEventHandler<TEvent>> _logger;

        public DomainEventHandler(ILogger<DomainEventHandler<TEvent>> logger)
        {
            _logger = logger;
        }

        public virtual async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling domain event: {EventType}", typeof(TEvent).Name);

                await HandleEventAsync(@event, cancellationToken);

                _logger.LogInformation("Domain event handled successfully: {EventType}", typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event: {EventType}", typeof(TEvent).Name);
                throw;
            }
        }

        // 📌 متد انتزاعی برای منطق اختصاصی پردازش رویداد
        protected abstract Task HandleEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
