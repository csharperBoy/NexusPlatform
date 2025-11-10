using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Core.Domain.Common
{
    /*
     📌 OutboxMessage
     ----------------
     این کلاس مدل اصلی برای پیاده‌سازی **Outbox Pattern** در معماری DDD است.
     هدف آن ذخیره‌سازی رویدادهای دامنه (Domain Events) به صورت پایدار در دیتابیس
     و مدیریت وضعیت پردازش آن‌ها می‌باشد.

     ✅ نکات کلیدی:
     - TypeName → نام نوع رویداد (مثلاً "OrderCreatedEvent").
     - AssemblyQualifiedName → نام کامل نوع رویداد همراه با Assembly.
     - Content → محتوای سریال‌شده رویداد (JSON).
     - OccurredOnUtc → زمان وقوع رویداد (UTC).
     - ProcessedOnUtc → زمان پردازش رویداد (در صورت موفقیت).
     - ErrorMessage / ErrorStackTrace → اطلاعات خطا در صورت شکست پردازش.
     - Status → وضعیت پردازش رویداد (Pending, Processing, Completed, Failed).
     - RetryCount → تعداد دفعات تلاش مجدد برای پردازش.
     - EventVersion → نسخه‌ی رویداد (برای سازگاری در آینده).
     - RowVersion → فیلد Concurrency برای مدیریت همزمانی در EF Core.

     🛠 جریان کار (Outbox Pattern):
     1. موجودیت دامنه یک رویداد تولید می‌کند (مثلاً OrderCreatedEvent).
     2. رویداد در قالب OutboxMessage ذخیره می‌شود (Content = JSON).
     3. سرویس OutboxProcessor رویدادهای Pending را خوانده و پردازش می‌کند.
     4. اگر موفق شود → Status = Completed و ProcessedOnUtc ثبت می‌شود.
     5. اگر شکست بخورد → Status = Failed و RetryCount افزایش می‌یابد.
     6. RetryPolicy می‌تواند دوباره تلاش کند تا رویداد منتشر شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Reliable Event Publishing** در معماری ماژولار است
     و تضمین می‌کند که هیچ رویدادی از دست نرود حتی اگر سرویس یا شبکه دچار خطا شود.
    */

    public class OutboxMessage : BaseEntity
    {
        // 📌 نوع رویداد
        public string TypeName { get; private set; } = string.Empty;
        public string AssemblyQualifiedName { get; private set; } = string.Empty;

        // 📌 محتوای سریال‌شده (JSON)
        public string Content { get; private set; } = string.Empty;

        // 📌 زمان وقوع رویداد
        public DateTime OccurredOnUtc { get; private set; } = DateTime.UtcNow;

        // 📌 زمان پردازش
        public DateTime? ProcessedOnUtc { get; private set; }

        // 📌 اطلاعات خطا
        public string? ErrorMessage { get; private set; }
        public string? ErrorStackTrace { get; private set; }

        // 📌 وضعیت پردازش
        public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;
        public int RetryCount { get; private set; }

        // 📌 نسخه‌ی رویداد
        public int EventVersion { get; private set; } = 1;

        // 📌 Optimistic Concurrency
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        // EF نیاز به سازنده‌ی خصوصی دارد
        private OutboxMessage() { }

        public OutboxMessage(IDomainEvent domainEvent, int eventVersion = 1)
        {
            var type = domainEvent.GetType();
            TypeName = type.Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;

            Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, type);

            OccurredOnUtc = domainEvent.OccurredOn;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
            EventVersion = eventVersion;
        }

        // 📌 تغییر وضعیت‌ها
        public void MarkAsProcessing() => Status = OutboxMessageStatus.Processing;

        public void MarkAsCompleted()
        {
            Status = OutboxMessageStatus.Completed;
            ProcessedOnUtc = DateTime.UtcNow;
        }

        public void MarkAsFailed(Exception ex)
        {
            Status = OutboxMessageStatus.Failed;
            ErrorMessage = ex.Message;
            ErrorStackTrace = ex.StackTrace;
            RetryCount++;
        }
    }

    // 📌 وضعیت‌های OutboxMessage
    public enum OutboxMessageStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
