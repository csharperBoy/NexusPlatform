using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions
{
    /*
     📌 IUnitOfWork<TContext>
     ------------------------
     این اینترفیس قرارداد **Unit of Work Pattern** در معماری Clean/DDD است.
     هدف آن مدیریت تراکنش‌ها و ذخیره تغییرات در DbContext به صورت هماهنگ و قابل کنترل می‌باشد.

     ✅ نکات کلیدی:
     - Generic Interface:
       → TContext : DbContext → هر DbContext اختصاصی ماژول.
     - از IDisposable و IAsyncDisposable ارث‌بری می‌کند → مدیریت چرخه عمر و آزادسازی منابع.

     - متدها:
       1. BeginTransactionAsync()
          → شروع یک تراکنش جدید در DbContext.
          → برای سناریوهایی که چند عملیات باید در یک تراکنش انجام شوند.

       2. SaveChangesAsync(CancellationToken cancellationToken = default)
          → ذخیره تغییرات و Commit تراکنش (در صورت وجود).
          → این متد معمولاً Outbox Events را هم ذخیره می‌کند.

       3. SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default)
          → ذخیره تغییرات بدون Commit تراکنش.
          → برای سناریوهایی که نیاز به ذخیره موقت داده‌ها داریم ولی هنوز نمی‌خواهیم تراکنش نهایی شود.

       4. RollbackAsync()
          → بازگردانی تراکنش (Rollback) در صورت خطا یا لغو عملیات.

       5. Context
          → دسترسی مستقیم به DbContext برای موارد خاص (مثلاً اجرای Queryهای سفارشی).
          → توصیه می‌شود استفاده از این ویژگی محدود باشد تا اصل جداسازی لایه‌ها حفظ شود.

     🛠 جریان کار:
     1. سرویس‌های Command یا Handlerها عملیات نوشتن روی Repository انجام می‌دهند.
     2. تغییرات در DbContext نگه‌داری می‌شوند.
     3. UnitOfWork متد SaveChangesAsync را فراخوانی می‌کند تا تغییرات ذخیره و تراکنش Commit شود.
     4. در صورت خطا، RollbackAsync فراخوانی می‌شود تا تراکنش بازگردانده شود.
     5. Outbox Pattern معمولاً در SaveChangesAsync ادغام می‌شود تا رویدادهای دامنه ذخیره و منتشر شوند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Transaction Management + Outbox Integration** در معماری ماژولار است
     و تضمین می‌کند که عملیات‌های نوشتن به صورت اتمیک و قابل اعتماد انجام شوند.
    */

    public interface IUnitOfWork<TContext> : IDisposable, IAsyncDisposable
        where TContext : DbContext
    {
        /// <summary>
        /// 📌 شروع یک تراکنش جدید
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 📌 ذخیره تغییرات و Commit تراکنش (در صورت وجود)
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 📌 ذخیره تغییرات بدون Commit تراکنش
        /// </summary>
        Task<int> SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 📌 Rollback تراکنش (در صورت وجود)
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// 📌 دسترسی مستقیم به DbContext (در صورت نیاز)
        /// </summary>
        TContext Context { get; }
    }
}
