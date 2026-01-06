using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions
{
    /*
     📌 IRepository<TDbContext, TEntity, TKey>
     -----------------------------------------
     این اینترفیس قرارداد عمومی برای Repository در معماری Clean/DDD است.
     هدف آن جداسازی لایه Application و Domain از جزئیات پیاده‌سازی EF Core می‌باشد
     (Slimmed Repository → جلوگیری از نشت نوع‌های EF به لایه‌های بالاتر).

     ✅ نکات کلیدی:
     - Generic Interface:
       → TDbContext : DbContext → هر DbContext اختصاصی ماژول.
       → TEntity : class → موجودیت دامنه.
       → TKey : IEquatable<TKey> → کلید اصلی موجودیت (مثلاً Guid یا int).

     - متدها:
       🔹 Basic CRUD:
         1. GetByIdAsync(TKey id) → دریافت موجودیت بر اساس کلید.
         2. GetAllAsync() → دریافت همه موجودیت‌ها.
         3. AddAsync(TEntity entity) → افزودن موجودیت جدید.
         4. AddRangeAsync(IEnumerable<TEntity> entities) → افزودن چند موجودیت.
         5. UpdateAsync(TEntity entity) → بروزرسانی موجودیت.
         6. DeleteAsync(TKey id) → حذف موجودیت بر اساس کلید.
         7. DeleteAsync(TEntity entity) → حذف موجودیت بر اساس نمونه.
         8. RemoveRangeAsync(IEnumerable<TEntity> entities) → حذف چند موجودیت.

       🔹 Query Operations:
         9. ExistsAsync(Expression<Func<TEntity, bool>> predicate) → بررسی وجود داده بر اساس شرط.
         10. CountAsync(Expression<Func<TEntity, bool>>? predicate = null) → شمارش موجودیت‌ها (با یا بدون شرط).

       🔹 Queryable Access:
         11. AsQueryable() → دسترسی به IQueryable برای Queryهای سفارشی.
         12. AsNoTrackingQueryable() → دسترسی به IQueryable بدون Tracking (برای خواندن سریع‌تر).

     🛠 جریان کار:
     1. سرویس‌های Application یا Handlerها از IRepository برای CRUD و Query استفاده می‌کنند.
     2. پیاده‌سازی در لایه Infrastructure انجام می‌شود (مثلاً EfRepository).
     3. UnitOfWork تغییرات را ذخیره می‌کند و رویدادهای دامنه را به Outbox اضافه می‌کند.
     4. اینترفیس IRepository تضمین می‌کند که Application فقط قرارداد را بشناسد، نه EF Core.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Repository Pattern** در معماری ماژولار است و
     باعث می‌شود لایه Application و Domain مستقل از EF Core باشند.
    */

    // Slimmed repository to avoid EF type leakage
    public interface IRepository<TDbContext, TEntity, TKey>
       where TDbContext : DbContext
       where TEntity : class
       where TKey : IEquatable<TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
        Task DeleteAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        IQueryable<TEntity> AsQueryable();
        IQueryable<TEntity> AsNoTrackingQueryable();
    }
}
