using Core.Application.Abstractions;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
namespace Core.Infrastructure.Repositories
{
    /*
     📌 EfRepository<TDbContext, TEntity, TKey>
     ------------------------------------------
     این کلاس یک پیاده‌سازی عمومی از Repository Pattern با استفاده از EF Core است.
     هدف آن جداسازی منطق دسترسی به داده‌ها از لایه‌های بالاتر (Domain, Application)
     و ایجاد یک API استاندارد برای عملیات CRUD و Query روی موجودیت‌ها می‌باشد.

     ✅ نکات کلیدی:
     - Generic Parameters:
       • TDbContext → نوع DbContext که دیتابیس را مدیریت می‌کند.
       • TEntity → نوع موجودیت (Entity) که روی آن عملیات انجام می‌شود.
       • TKey → نوع کلید اصلی موجودیت (مثلاً int, Guid).

     - سازنده:
       • DbContext تزریق می‌شود و DbSet<TEntity> ساخته می‌شود.
       • این طراحی باعث می‌شود Repository مستقل از نوع موجودیت باشد.

     - متدها:
       • GetByIdAsync → دریافت موجودیت بر اساس کلید اصلی.
       • GetAllAsync → دریافت همه‌ی موجودیت‌ها.
       • AddAsync / AddRangeAsync → افزودن یک یا چند موجودیت.
       • UpdateAsync → بروزرسانی موجودیت.
       • DeleteAsync(id) → حذف موجودیت بر اساس کلید.
       • DeleteAsync(entity) → حذف موجودیت مشخص.
       • RemoveRangeAsync → حذف چند موجودیت.
       • ExistsAsync(predicate) → بررسی وجود موجودیت بر اساس شرط.
       • CountAsync(predicate) → شمارش موجودیت‌ها (با یا بدون شرط).
       • AsQueryable → بازگرداندن IQueryable برای Queryهای سفارشی.
       • AsNoTrackingQueryable → بازگرداندن IQueryable بدون Tracking (برای خواندن سریع‌تر).

     🛠 جریان کار:
     1. سرویس‌های Application یا Domain به جای کار مستقیم با DbContext از IRepository استفاده می‌کنند.
     2. این کلاس پیاده‌سازی عمومی IRepository است و همه‌ی عملیات پایه را فراهم می‌کند.
     3. اگر نیاز به عملیات خاص باشد، می‌توان Repository اختصاصی ایجاد کرد و از این کلاس ارث‌بری کرد.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Generic Repository Pattern with EF Core** در معماری ماژولار است
     و تضمین می‌کند که دسترسی به داده‌ها به صورت استاندارد، قابل تست و قابل توسعه انجام شود.
    */

    public class EfRepository<TDbContext, TEntity, TKey> : IRepository<TDbContext, TEntity, TKey>
       where TDbContext : DbContext
       where TEntity : class
       where TKey : IEquatable<TKey>
    {
        protected readonly TDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public EfRepository(TDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id) =>
            await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        public virtual async Task AddAsync(TEntity entity) =>
            await _dbSet.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities) =>
            await _dbSet.AddRangeAsync(entities);

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public virtual IQueryable<TEntity> AsQueryable() => _dbSet;

        public virtual IQueryable<TEntity> AsNoTrackingQueryable() => _dbSet.AsNoTracking();
    }
}
