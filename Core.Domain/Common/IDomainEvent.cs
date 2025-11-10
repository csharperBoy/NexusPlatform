using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Common
{
    /*
     📌 IDomainEvent
     ----------------
     این اینترفیس قرارداد پایه برای همه‌ی رویدادهای دامنه (Domain Events) است.
     هدف آن استانداردسازی ساختار رویدادها و تضمین سازگاری آن‌ها با مکانیزم
     انتشار رویدادها در معماری DDD می‌باشد.

     ✅ نکات کلیدی:
     - از INotification ارث‌بری می‌کند:
       → این کار باعث می‌شود رویدادها بتوانند توسط MediatR منتشر شوند.
       → MediatR از INotification برای انتشار Eventها به Handlerهای مربوطه استفاده می‌کند.

     - OccurredOn → زمان وقوع رویداد:
       → هر رویداد باید زمان وقوع خود را ثبت کند.
       → این ویژگی برای لاگ‌گذاری، بازپخش رویدادها (Event Sourcing)، و تحلیل سیستم ضروری است.

     🛠 جریان کار:
     1. یک موجودیت دامنه (Entity) رویدادی تولید می‌کند (مثلاً OrderCreatedEvent).
     2. این رویداد از IDomainEvent ارث‌بری می‌کند تا OccurredOn و قابلیت انتشار داشته باشد.
     3. Event Dispatcher یا Outbox این رویداد را منتشر می‌کند.
     4. Handlerهای مربوطه (DomainEventHandlerها) آن را پردازش می‌کنند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Domain Events** در معماری DDD است و تضمین می‌کند
     که همه‌ی رویدادها دارای زمان وقوع استاندارد باشند و بتوانند توسط MediatR منتشر شوند.
    */

    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; } // 📌 زمان وقوع رویداد
    }
}
