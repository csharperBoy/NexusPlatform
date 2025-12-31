using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
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
        protected readonly IDataScopeProcessor _scopeProcessor;
        protected readonly ICurrentUserService _currentUserService;

        // تغییر ۱: حذف تزریق مستقیم IAuthorizationChecker و جایگزینی با IServiceProvider
        protected readonly IServiceProvider _serviceProvider;

        public EfRepository(
            TDbContext dbContext,
            IDataScopeProcessor scopeProcessor,
            ICurrentUserService currentUserService,
            IServiceProvider serviceProvider // تغییر ۱
            )
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
            _scopeProcessor = scopeProcessor;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
        }

        // ... (متدهای GetByIdAsync, GetAllAsync, CountAsync, ExistsAsync بدون تغییر) ...
        // فقط دقت کنید در متد GetByIdAsync و ... از _scopeProcessor استفاده کرده‌اید که عالی است
        // اما حواستان باشد خود ScopeProcessor هم باید برای Permission چک نشود (که در پاسخ قبلی حل کردیم)

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var result = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            return await result.ToListAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            if (predicate != null) query = query.Where(predicate);
            return await query.CountAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            return await query.AnyAsync(predicate);
        }

        // --- WRITE OPERATIONS ---

        public virtual async Task AddAsync(TEntity entity)
        {
            SetOwnerDefaults(entity);
            await CheckPermissionAsync(entity, PermissionAction.Create);
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetOwnerDefaults(entity);
                await CheckPermissionAsync(entity, PermissionAction.Create);
            }
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            await CheckPermissionAsync(entity, PermissionAction.Edit);
            _dbSet.Update(entity);
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return;

            await CheckPermissionAsync(entity, PermissionAction.Delete);
            _dbSet.Remove(entity);
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            await CheckPermissionAsync(entity, PermissionAction.Delete);
            if (_dbContext.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);
            _dbSet.Remove(entity);
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await CheckPermissionAsync(entity, PermissionAction.Delete);
            }
            _dbSet.RemoveRange(entities);
        }

        public virtual IQueryable<TEntity> AsQueryable() => _dbSet;
        public virtual IQueryable<TEntity> AsNoTrackingQueryable() => _dbSet.AsNoTracking();

        // --- SECURITY LOGIC ---

        protected virtual async Task CheckPermissionAsync(TEntity entity, PermissionAction action)
        {
            // تغییر ۲: جلوگیری از لوپ منطقی (بسیار مهم)
            // اگر داریم روی جدول Permission یا Resource عملیات انجام می‌دهیم، نباید چک کنیم
            // چون خود AuthorizationService برای چک کردن نیاز به خواندن اینها دارد.
           /* if (typeof(TEntity) == typeof(Permission) ||
                typeof(TEntity) == typeof(Resource) ||
                typeof(TEntity) == typeof(UserSession)) // UserSession هم در زنجیره لاگین است
            {
                return;
            }*/

            var resourceAttr = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (resourceAttr == null) return;

            if (entity is not IDataScopedEntity dataScopedEntity) return;

            var userId = _currentUserService.UserId;
            if (userId == null) return;

            // تغییر ۳: دریافت سرویس فقط در لحظه نیاز (Lazy Resolution)
            // این کار باعث می‌شود در لحظه ساخت Repository، نیازی به ساخت AuthorizationService نباشد
            // و Circular Dependency در استارتاپ حل شود.
            var authorizationChecker = _serviceProvider.GetRequiredService<IAuthorizationChecker>();

            var scope = await authorizationChecker.GetPermissionScopeAsync(userId.Value, resourceAttr.ResourceKey, action);

            bool isAllowed = IsEntityInScope(dataScopedEntity, scope, userId.Value);

            if (!isAllowed)
            {
                throw new UnauthorizedAccessException($"User does not have permission to {action} this resource due to data scope restrictions.");
            }
        }

        // ... (متدهای IsEntityInScope و SetOwnerDefaults بدون تغییر) ...
        private bool IsEntityInScope(IDataScopedEntity entity, ScopeType scope, Guid userId)
        {
            switch (scope)
            {
                case ScopeType.All:
                    return true;
                case ScopeType.Self:
                    return entity.OwnerPersonId == userId;
                case ScopeType.Unit:
                    var userUnitId = _currentUserService.OrganizationUnitId;
                    return entity.OwnerOrganizationUnitId == userUnitId;
                case ScopeType.UnitAndBelow:
                    var uUnitId = _currentUserService.OrganizationUnitId;
                    return entity.OwnerOrganizationUnitId == uUnitId;
                case ScopeType.None:
                default:
                    return false;
            }
        }

        private void SetOwnerDefaults(TEntity entity)
        {
            if (entity is DataScopedEntity scopedEntity)
            {
                if (scopedEntity.OwnerPersonId == null || scopedEntity.OwnerPersonId == Guid.Empty)
                {
                    var personId = _currentUserService.UserId ?? Guid.Empty;
                    scopedEntity.SetPersonOwner(personId);
                }
                if (scopedEntity.OwnerOrganizationUnitId == null || scopedEntity.OwnerOrganizationUnitId == Guid.Empty)
                {
                    var orgUnitId = _currentUserService.OrganizationUnitId ?? Guid.Empty;
                    scopedEntity.SetOrganizationUnitOwner(orgUnitId);
                }
                if (scopedEntity.OwnerPositionId == null || scopedEntity.OwnerPositionId == Guid.Empty)
                {
                    var positionId = _currentUserService.PositionId ?? Guid.Empty;
                    scopedEntity.SetPositionOwner(positionId);
                }
            }
        }
    }
}
