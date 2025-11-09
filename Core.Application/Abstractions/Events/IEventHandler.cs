using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Events
{
    /*
     📌 IEventHandler<TEvent>
     ------------------------
     این اینترفیس قرارداد عمومی برای **پردازش رویدادهای دامنه (Domain Events)** است.
     هدف آن جداسازی منطق واکنش به رویدادها از منطق اصلی سرویس‌ها و موجودیت‌ها می‌باشد.

     ✅ نکات کلیدی:
     - Generic Interface → برای هر نوع رویداد دامنه (TEvent) یک Handler جداگانه تعریف می‌شود.
     - محدودیت TEvent : IDomainEvent → فقط رویدادهایی که از IDomainEvent ارث‌بری کرده‌اند قابل پردازش هستند.
     - متد اصلی: HandleAsync
       → وظیفه دارد منطق واکنش به رویداد را اجرا کند.
       → پارامترها:
         1. @event → نمونه‌ی رویداد دامنه که باید پردازش شود.
         2. cancellationToken → برای مدیریت لغو عملیات (اختیاری).

     🛠 جریان کار:
     1. موجودیت‌ها در لایه Domain رویداد تولید می‌کنند (مثلاً SampleCreatedIntegrationEvent).
     2. این رویدادها توسط EventBus یا Outbox منتشر می‌شوند.
     3. EventBus یک Handler مناسب برای رویداد پیدا می‌کند.
     4. متد HandleAsync در Handler فراخوانی می‌شود.
     5. منطق واکنش اجرا می‌شود (مثل ارسال ایمیل، ثبت لاگ، هماهنگی با سرویس دیگر).

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Event Handling** در معماری ماژولار است و تضمین می‌کند
     که هر رویداد دامنه یک Handler اختصاصی داشته باشد. این کار باعث رعایت اصل **Single Responsibility**
     و جداسازی منطق واکنش از منطق اصلی می‌شود.
    */

    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(
            TEvent @event,                          // 📌 رویداد دامنه
            CancellationToken cancellationToken = default // 📌 لغو عملیات (اختیاری)
        );
    }
}
