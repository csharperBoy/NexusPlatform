using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Events
{
    /*
     📌 IOutboxProcessorRegistration
     --------------------------------
     این اینترفیس قرارداد ثبت (Registration) پردازشگر Outbox در DI Container است.
     هدف آن فراهم کردن یک نقطه‌ی استاندارد برای اضافه کردن OutboxProcessor به سرویس‌ها می‌باشد.

     ✅ نکات کلیدی:
     - Outbox Pattern:
       → برای اطمینان از انتشار قابل اعتماد رویدادها استفاده می‌شود.
       → رویدادها ابتدا در دیتابیس ذخیره می‌شوند (Outbox Table).
       → سپس OutboxProcessor آن‌ها را خوانده و منتشر می‌کند.
     - این اینترفیس تضمین می‌کند که هر DbContext بتواند OutboxProcessor مخصوص به خودش را رجیستر کند.
     - متد اصلی: AddOutboxProcessor<TDbContext>
       → یک OutboxProcessor برای DbContext مشخص رجیستر می‌کند.
       → پارامتر services → DI Container که سرویس‌ها در آن رجیستر می‌شوند.
       → محدودیت TDbContext : DbContext → فقط DbContextهای EF Core پشتیبانی می‌شوند.

     🛠 جریان کار:
     1. در لایه Infrastructure، پیاده‌سازی این اینترفیس ارائه می‌شود.
     2. در زمان راه‌اندازی برنامه (Startup/Program.cs)، متد AddOutboxProcessor فراخوانی می‌شود.
     3. OutboxProcessor به DI Container اضافه می‌شود.
     4. OutboxProcessor در پس‌زمینه اجرا شده و رویدادهای ذخیره‌شده در Outbox Table را منتشر می‌کند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Outbox Registration** است و تضمین می‌کند
     که ماژول‌ها بتوانند به صورت ماژولار OutboxProcessor خود را رجیستر کنند،
     بدون اینکه به جزئیات پیاده‌سازی وابسته باشند.
    */

    public interface IOutboxProcessorRegistration
    {
        IServiceCollection AddOutboxProcessor<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext;
    }
}
