using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Shared.Events
{
    /*
     📌 SampleCreatedIntegrationEvent
     --------------------------------
     این کلاس یک **Integration Event** بین‌ماژولی است.
     هدف آن اطلاع‌رسانی به سایر ماژول‌ها یا سرویس‌هاست که یک Sample جدید ایجاد شده است.

     ✅ نکات کلیدی:
     - از IDomainEvent ارث‌بری می‌کند → بنابراین توسط مکانیزم Outbox در Core ذخیره و منتشر می‌شود.
     - این ایونت در لایه Shared قرار دارد → یعنی قابل استفاده در سایر ماژول‌هاست.
     - شامل اطلاعات کلیدی درباره Sample ایجاد شده:
       1. SampleId → شناسه موجودیت ایجاد شده.
       2. Property1 → مقدار ویژگی اصلی موجودیت.
       3. OccurredOn → زمان وقوع رویداد.

     🛠 جریان کار:
     1. وقتی یک Sample جدید در سرویس ایجاد می‌شود (مثلاً در SampleService).
     2. موجودیت SampleEntity یک Domain Event تولید می‌کند.
     3. این Domain Event به Outbox اضافه می‌شود.
     4. OutboxProcessor آن را به عنوان Integration Event منتشر می‌کند.
     5. سایر ماژول‌ها یا سرویس‌ها می‌توانند این ایونت را دریافت کرده و واکنش نشان دهند
        (مثلاً ارسال نوتیفیکیشن، به‌روزرسانی گزارش‌ها، یا هماهنگی با سرویس‌های دیگر).

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور می‌توان رویدادهای دامنه را به رویدادهای بین‌ماژولی تبدیل کرد
     تا ارتباط بین ماژول‌ها به صورت **loosely coupled** و استاندارد برقرار شود.
    */

    public class SampleCreatedIntegrationEvent : IDomainEvent
    {
        public Guid SampleId { get; }
        public string Property1 { get; }
        public DateTime OccurredOn { get; }

        public SampleCreatedIntegrationEvent(Guid sampleId, string property1, DateTime occurredOn)
        {
            SampleId = sampleId;
            Property1 = property1;
            OccurredOn = occurredOn;
        }
    }
}
