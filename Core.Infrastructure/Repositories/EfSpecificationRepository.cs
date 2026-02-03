using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Core.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    /*
     📌 EfSpecificationRepository<TDbContext, TEntity, TKey>
     -------------------------------------------------------
     این کلاس پیاده‌سازی عمومی (Generic Implementation) برای **Specification Repository Pattern**
     با استفاده از EF Core است. هدف آن جداسازی منطق Queryهای پیچیده از سرویس‌ها و
     فراهم کردن یک API استاندارد برای اعمال Specification روی موجودیت‌ها می‌باشد.

     ✅ نکات کلیدی:
     - Generic Parameters:
       • TDbContext → نوع DbContext که دیتابیس را مدیریت می‌کند.
       • TEntity → نوع موجودیت (Entity) که روی آن عملیات انجام می‌شود.
       • TKey → نوع کلید اصلی موجودیت (مثلاً int, Guid).

     - سازنده:
       • DbContext تزریق می‌شود و DbSet<TEntity> ساخته می‌شود.
       • این طراحی باعث می‌شود Repository مستقل از نوع موجودیت باشد.

     - متدها:
       • GetBySpecAsync → دریافت اولین موجودیت که با Specification مطابقت دارد.
       • ListBySpecAsync → دریافت لیست موجودیت‌ها بر اساس Specification.
       • FindBySpecAsync → دریافت لیست موجودیت‌ها + شمارش کل (برای Paging).
       • CountBySpecAsync → شمارش موجودیت‌ها بر اساس Specification.
       • ApplySpecification → اعمال Criteria, Includes, IncludeFunctions, IncludeStrings و Ordering روی Query.
       • ApplyOrdering → اعمال OrderBy, OrderByDescending و ThenOrderBy روی Query.

     - ویژگی‌ها:
       • پشتیبانی از Criteria (فیلترها).
       • پشتیبانی از Includes (برای eager loading).
       • پشتیبانی از IncludeFunctions (برای custom include).
       • پشتیبانی از IncludeStrings (برای include با نام رشته‌ای).
       • پشتیبانی از Paging (Skip/Take).
       • پشتیبانی از Ordering و ThenOrdering.

     🛠 جریان کار:
     1. سرویس‌های Application یا Domain یک Specification تعریف می‌کنند (مثلاً "کاربران فعال با نقش Admin").
     2. این Specification شامل Criteria, Includes و Ordering است.
     3. Repository این Specification را اعمال می‌کند و Query نهایی ساخته می‌شود.
     4. نتیجه به صورت لیست، موجودیت واحد یا همراه با شمارش کل بازگردانده می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Specification Pattern with EF Core** در معماری ماژولار است
     و تضمین می‌کند که Queryهای پیچیده به صورت قابل تست، قابل استفاده مجدد و قابل نگهداری مدیریت شوند.
    */

    public class EfSpecificationRepository<TDbContext, TEntity, TKey> : ISpecificationRepository<TEntity, TKey>
        where TDbContext : DbContext
        where TEntity : class
        where TKey : IEquatable<TKey>
    {
        protected readonly TDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly IDataScopeProcessor _scopeProcessor;
        protected readonly ILogger<EfSpecificationRepository<TDbContext, TEntity, TKey>> _logger;

        public EfSpecificationRepository(TDbContext dbContext, IDataScopeProcessor scopeProcessor, ILogger<EfSpecificationRepository<TDbContext, TEntity, TKey>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;   
            _dbSet = dbContext.Set<TEntity>();
            _scopeProcessor = scopeProcessor;
        }
        private async Task<IQueryable<TEntity>> ApplySpecification(ISpecification<TEntity> specification)
        {
            try
            {


                IQueryable<TEntity> query = _dbSet;

                // ***** اعمال امنیت همین اول کار *****
                // اگر Specification خاصی نباید فیلتر شود، می‌توان پراپرتی IgnoreSecurity به Specification اضافه کرد
                query = await _scopeProcessor.ApplyScope(query);

                if (specification.Criteria != null)
                    query = query.Where(specification.Criteria);

                query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
                query = specification.IncludeFunctions.Aggregate(query, (current, includeFunction) => includeFunction(current));
                query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

                query = ApplyOrdering(query, specification);

                return query;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual async Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> specification)
        {
            try
            {

                var result = await ApplySpecification(specification);
                return await result.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual async Task<IEnumerable<TEntity>> ListBySpecAsync(ISpecification<TEntity> specification)
        {
            try
            {


                _logger.LogInformation(
                      "DbContext Hash befor ApplySpecification: {Hash}",
                      _dbContext.GetHashCode()
                    );
                var result =await ApplySpecification(specification);

                _logger.LogInformation(
                      "DbContext Hash after ApplySpecification: {Hash}",
                      _dbContext.GetHashCode()
                    );

                var a = await result.ToListAsync();
                _logger.LogInformation(
                      "DbContext Hash after ToListAsync: {Hash}",
                      _dbContext.GetHashCode()
                    );
                return a;
            }
            catch (Exception ex)
            {
                // خطا را بهتر لاگ کنید
                _logger.LogError(ex, "Error in ListBySpecAsync. Specification: {Spec}", specification);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindBySpecAsync(ISpecification<TEntity> specification)
        {
            // 📌 شمارش فقط با Criteria برای جلوگیری از بارگذاری سنگین Includes
            var countQuery = _dbSet.AsQueryable();
            countQuery = await _scopeProcessor.ApplyScope(countQuery);
            if (specification.Criteria != null)
                countQuery = countQuery.Where(specification.Criteria);
            var totalCount = await countQuery.CountAsync();

            var query = await ApplySpecification(specification);
            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public virtual async Task<int> CountBySpecAsync(ISpecification<TEntity> specification)
        {
            var query = _dbSet.AsQueryable();
            query = await _scopeProcessor.ApplyScope(query);
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            return await query.CountAsync();
        }



        private IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, ISpecification<TEntity> specification)
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;

            if (specification.OrderBy != null)
                orderedQuery = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                orderedQuery = query.OrderByDescending(specification.OrderByDescending);

            if (orderedQuery != null && specification.ThenOrderBy.Any())
            {
                foreach (var (keySelector, isDescending) in specification.ThenOrderBy)
                {
                    orderedQuery = isDescending
                        ? orderedQuery.ThenByDescending(keySelector)
                        : orderedQuery.ThenBy(keySelector);
                }
                return orderedQuery;
            }

            return orderedQuery ?? query;
        }
    }
}
