using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Common
{
    /*
     📌 DomainEventBase
     ------------------
     این کلاس یک **Base Class** برای همه‌ی رویدادهای دامنه (Domain Events) است.
     هدف آن فراهم کردن یک اسکلت استاندارد برای ثبت زمان وقوع رویداد و
     تضمین سازگاری همه‌ی رویدادها با قرارداد IDomainEvent می‌باشد.

     ✅ نکات کلیدی:
     - از IDomainEvent ارث‌بری می‌کند → همه رویدادها باید این قرارداد را رعایت کنند.
     - OccurredOn → زمان وقوع رویداد:
       • به صورت خودکار مقداردهی می‌شود (DateTime.UtcNow).
       • امکان مقداردهی دستی نیز وجود دارد (مثلاً برای تست یا بازپخش رویدادها).
     - سازنده (Constructor):
       • اگر زمان وقوع مشخص نشده باشد، مقدار پیش‌فرض زمان فعلی UTC خواهد بود.
       • اگر زمان وقوع مشخص شود، همان مقدار استفاده می‌شود.

     🛠 جریان کار:
     1. یک موجودیت دامنه (Entity) رویدادی تولید می‌کند (مثلاً OrderCreatedEvent).
     2. این رویداد از DomainEventBase ارث‌بری می‌کند تا OccurredOn به صورت استاندارد ثبت شود.
     3. Event Dispatcher یا Outbox این رویداد را منتشر می‌کند.
     4. زمان وقوع رویداد همیشه در دسترس است و می‌تواند برای لاگ‌گذاری یا بازپخش استفاده شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Domain Events** در معماری DDD است و تضمین می‌کند
     که همه‌ی رویدادها دارای زمان وقوع استاندارد باشند.
     این کار باعث افزایش قابلیت ردیابی (Traceability) و شفافیت در سیستم می‌شود.
    */

    // Strong base for domain events
    public abstract class DomainEventBase : IDomainEvent
    {
        public DateTime OccurredOn { get; }

        protected DomainEventBase(DateTime? occurredOn = null)
        {
            OccurredOn = occurredOn ?? DateTime.UtcNow; // 📌 زمان وقوع رویداد
        }
    }
}
