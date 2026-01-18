using Core.Domain.Common;
using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using Sample.Domain.Events;
namespace Sample.Domain.Entities
{
    /*
     📌 SampleEntity
     ----------------
     این کلاس یک موجودیت (Entity) در لایه Domain است.
     موجودیت‌ها قلب معماری DDD هستند و وضعیت (State) و رفتار (Behavior) مرتبط با یک مفهوم تجاری را نگه می‌دارند.

     ✅ نکات کلیدی:
     - از AuditableEntity ارث‌بری می‌کند، بنابراین ویژگی‌های ثبت زمان ایجاد/ویرایش و کاربر ایجادکننده/ویرایش‌کننده را دارد.
     - property1 یک ویژگی ساده است که وضعیت موجودیت را نگه می‌دارد.
     - ValueObjectsSample یک نمونه از Value Object (اینجا PhoneNumber) است که نشان می‌دهد
       موجودیت می‌تواند داده‌های پیچیده‌تر را در قالب Value Object نگه دارد.
       Value Objectها رفتار و قوانین اعتبارسنجی خودشان را دارند و مستقل از موجودیت قابل استفاده‌اند.
     - متد MarkSample یک رفتار دامنه‌ای (Domain Behavior) است که علاوه بر تغییر وضعیت، یک Domain Event تولید می‌کند.

     🛠 جریان کار:
     1. وقتی متد MarkSample فراخوانی می‌شود:
        - مقدار property1 تغییر می‌کند.
        - یک Domain Event از نوع SampleActionEvent تولید می‌شود.
     2. این رویداد در لیست DomainEvents موجودیت ذخیره می‌شود.
     3. بعداً توسط EventBus یا Outbox منتشر می‌شود و Handlerهای مربوطه آن را پردازش می‌کنند.

     📌 نتیجه:
     این کلاس نشان می‌دهد که موجودیت‌ها در DDD فقط داده نیستند،
     بلکه رفتار دارند، می‌توانند رویدادهای دامنه تولید کنند،
     و می‌توانند از Value Objectها برای مدل‌سازی دقیق‌تر داده‌های تجاری استفاده کنند.
    */

    public class SampleEntity : DataScopedEntity,IAggregateRoot
    {
        // ویژگی ساده برای نگهداری وضعیت
        public string property1 { get; set; } = default!;

        // 📌 نمونه استفاده از Value Object در موجودیت
        // Value Object قوانین خودش را دارد و مستقل از موجودیت قابل استفاده است
        public PhoneNumber? ValueObjectsSample { get; private set; }

        // 📌 رفتار دامنه‌ای: تغییر وضعیت و تولید رویداد دامنه
        public void MarkSample(string value)
        {
            property1 = value;

            // تولید Domain Event و اضافه کردن آن به لیست رویدادهای موجودیت
            AddDomainEvent(new SampleActionEvent(value));
        }
    }
}
