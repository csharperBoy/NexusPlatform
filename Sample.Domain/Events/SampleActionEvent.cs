using Core.Domain.Common;
using Core.Shared.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Domain.Events
{
    /*
     📌 SampleActionEvent
     --------------------
     این کلاس یک Domain Event در معماری DDD است.
     رویدادهای دامنه زمانی تولید می‌شوند که یک تغییر مهم در وضعیت موجودیت رخ دهد
     و سایر بخش‌های سیستم باید از آن مطلع شوند.

     ✅ نکات کلیدی:
     - از IDomainEvent ارث‌بری می‌کند، بنابراین با EventBus و Outbox سازگار است.
     - ActionProperty1 داده‌ای است که نشان‌دهنده‌ی جزئیات عملیات انجام‌شده است.
     - OccurredOn زمان وقوع رویداد را مشخص می‌کند.
     - این رویداد معمولاً توسط موجودیت SampleEntity در متد MarkSample تولید می‌شود.

     ⚠️ نکته مهم:
     - در کد اولیه، فیلد ITimeProvider تعریف شده ولی مقداردهی نمی‌شود → خطای NullReferenceException.
     - راه‌حل: یا ITimeProvider را به سازنده تزریق کنید، یا از DateTime.UtcNow استفاده کنید.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید یک Domain Event تعریف کنیم تا تغییرات مهم در سیستم
     به سایر بخش‌ها اطلاع داده شود.
    */

    public class SampleActionEvent : IDomainEvent
    {
        public string ActionProperty1 { get; }
        public DateTime OccurredOn { get; }

        // ✅ نسخه اصلاح‌شده: استفاده مستقیم از DateTime.UtcNow
        public SampleActionEvent(string actionProperty1)
        {
            ActionProperty1 = actionProperty1;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
