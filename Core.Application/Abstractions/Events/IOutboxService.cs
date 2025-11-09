using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Events
{
    /*
     📌 IOutboxService<TDbContext>
     -----------------------------
     این اینترفیس قرارداد سرویس Outbox در معماری Event-Driven است.
     هدف آن مدیریت چرخه‌ی عمر رویدادهای دامنه (Domain Events) در جدول Outbox می‌باشد،
     تا انتشار رویدادها به صورت قابل اعتماد و مقاوم در برابر خطا انجام شود.

     ✅ نکات کلیدی:
     - Outbox Pattern:
       → رویدادها ابتدا در دیتابیس ذخیره می‌شوند (Outbox Table).
       → سپس پردازشگر Outbox آن‌ها را خوانده و منتشر می‌کند.
       → این الگو از دست رفتن رویدادها در صورت خطا یا قطعی ارتباط جلوگیری می‌کند.

     - متدها:
       1. AddEventsAsync(IEnumerable<IDomainEvent> domainEvents)
          → افزودن رویدادهای دامنه به جدول Outbox.

       2. GetPendingMessagesAsync(int batchSize = 100)
          → دریافت پیام‌های منتشرنشده (Pending) برای پردازش.
          → امکان تعیین اندازه Batch برای پردازش دسته‌ای.

       3. MarkAsProcessingAsync(Guid messageId)
          → علامت‌گذاری پیام به عنوان "در حال پردازش".

       4. MarkAsCompletedAsync(Guid messageId)
          → علامت‌گذاری پیام به عنوان "پردازش موفق".

       5. MarkAsFailedAsync(Guid messageId, Exception ex)
          → علامت‌گذاری پیام به عنوان "پردازش ناموفق" همراه با جزئیات خطا.

       6. CleanupProcessedMessagesAsync(DateTime olderThan)
          → پاک‌سازی پیام‌های پردازش‌شده قدیمی‌تر از تاریخ مشخص.
          → برای جلوگیری از رشد بی‌رویه جدول Outbox.

     🛠 جریان کار:
     1. موجودیت‌ها رویداد دامنه تولید می‌کنند.
     2. رویدادها توسط UnitOfWork در جدول Outbox ذخیره می‌شوند.
     3. OutboxProcessor پیام‌های Pending را با استفاده از IOutboxService می‌خواند.
     4. پیام‌ها منتشر می‌شوند و وضعیت آن‌ها به Completed یا Failed تغییر می‌کند.
     5. پیام‌های قدیمی پاک‌سازی می‌شوند تا جدول Outbox سبک بماند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم Outbox در معماری ماژولار است و تضمین می‌کند
     که انتشار رویدادها به صورت قابل اعتماد، مقاوم در برابر خطا و قابل مانیتورینگ انجام شود.
     پیاده‌سازی آن در لایه Infrastructure خواهد بود (مثلاً OutboxService مبتنی بر EF Core).
    */

    public interface IOutboxService<TDbContext>
         where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        Task AddEventsAsync(IEnumerable<IDomainEvent> domainEvents); // 📌 افزودن رویدادها به Outbox
        Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100); // 📌 دریافت پیام‌های منتشرنشده
        Task MarkAsProcessingAsync(Guid messageId); // 📌 علامت‌گذاری به عنوان در حال پردازش
        Task MarkAsCompletedAsync(Guid messageId); // 📌 علامت‌گذاری به عنوان پردازش موفق
        Task MarkAsFailedAsync(Guid messageId, Exception ex); // 📌 علامت‌گذاری به عنوان پردازش ناموفق
        Task CleanupProcessedMessagesAsync(DateTime olderThan); // 📌 پاک‌سازی پیام‌های قدیمی پردازش‌شده
    }
}
