using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        //protected readonly IPermissionChecker _permissionChecker; 
        protected readonly IAuthorizationChecker _authorizationChecker; 
        public EfRepository(TDbContext dbContext, IDataScopeProcessor scopeProcessor, ICurrentUserService currentUserService,
            IAuthorizationChecker authorizationChecker
            )
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
            _scopeProcessor = scopeProcessor;
            _currentUserService = currentUserService;
            _authorizationChecker = authorizationChecker;
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            // FindAsync روی DbSet مستقیم کار می‌کند و IQueryable نیست که فیلتر کنیم.
            // بنابراین باید تبدیلش کنیم به FirstOrDefaultAsync

            //var query = await _scopeProcessor.ApplyScope(_dbSet);
            // از AsQueryable استفاده می‌کنیم تا Scope اعمال شود
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            // اینجا نیاز داریم شرط Id == id را بسازیم. 
            // چون TKey جنریک است، کمی پیچیده می‌شود. 
            // ساده‌ترین راه: اگر Entity اینترفیس IEntity<TKey> دارد:
            return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            // اعمال فیلتر امنیتی
            //return await _scopeProcessor.ApplyScope(_dbSet).ToListAsync(); 
            var result = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            return await result.ToListAsync(); 
        }
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            //var query = await _scopeProcessor.ApplyScope(_dbSet);
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            if (predicate != null) query = query.Where(predicate);
            return await query.CountAsync();
        }
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            // برای Exists هم باید اسکوپ اعمال شود
            // مثلا اگر کاربر چک کند "آیا فاکتور شماره ۱۰ وجود دارد؟"
            // اگر فاکتور ۱۰ مال او نباشد، باید False برگردد (انگار وجود ندارد)
            var query = await _scopeProcessor.ApplyScope(_dbSet.AsQueryable());
            return await query.AnyAsync(predicate);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            // 1. ست کردن اطلاعات مالکیت (اگر ست نشده باشد)
            SetOwnerDefaults(entity);

            // 2. چک کردن مجوز "Create"
            await CheckPermissionAsync(entity, PermissionAction.Create);

            await _dbSet.AddAsync(entity);
        }
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetOwnerDefaults(entity);
                // برای تک تک رکوردها باید چک شود (چون ممکن است کاربر بخواهد برای واحدهای مختلف رکورد بزند)
                await CheckPermissionAsync(entity, PermissionAction.Create);
            }
            await _dbSet.AddRangeAsync(entities);
        }
        public virtual async Task UpdateAsync(TEntity entity)
        {
            // 1. چک کردن مجوز "Edit"
            // نکته: اینجا چک می‌کنیم آیا کاربری که لاگین کرده، با توجه به اسکوپی که برای Edit دارد،
            // مجاز است این موجودیت (با این Owner و Unit فعلی) را ویرایش کند؟
            await CheckPermissionAsync(entity, PermissionAction.Edit);

            _dbSet.Update(entity);
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            // برای حذف، ابتدا باید موجودیت را بگیریم تا بفهمیم مالکش کیست
            // اینجا از GetByIdAsync خودمان استفاده نمی‌کنیم چون آن فقط Read Access را چک می‌کند.
            // ما نیاز به دیتای خام داریم تا مجوز Delete را رویش بسنجیم.

            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return; // یا پرتاب خطا

            // چک کردن مجوز "Delete"
            await CheckPermissionAsync(entity, PermissionAction.Delete);

            _dbSet.Remove(entity);
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            // اگر موجودیت Detached است، نمی‌توانیم به مقادیرش اعتماد کنیم، اما فرض بر این است
            // که از سرویس معتبر آمده. برای اطمینان مجدد:
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


        /// <summary>
        /// این متد مرکزی چک می‌کند که آیا کاربر جاری مجاز به انجام اکشن خاص روی این موجودیت هست یا خیر
        /// </summary>
        protected virtual async Task CheckPermissionAsync(TEntity entity, PermissionAction action)
        {
            // 1. اگر موجودیت SecuredResource نیست، یعنی دسترسی عمومی دارد یا کنترل نمی‌شود
            var resourceAttr = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (resourceAttr == null) return;

            // 2. اگر موجودیت DataScoped نیست، یعنی مالکیت ندارد، پس فقط رول چک می‌شود (که در لایه API انجام شده)
            // اما اگر بخواهید سخت‌گیری کنید می‌توانید اینجا هم چک کنید.
            if (entity is not IDataScopedEntity dataScopedEntity) return;

            var userId = _currentUserService.UserId;
            if (userId == null) return; // یا پرتاب UnauthorizedException

            // 3. دریافت Scope که کاربر برای این اکشن (مثلا Edit) روی این ریسورس دارد
            // مثال: کاربر برای ریسورس "Invoice"، اکشن "Edit" را با اسکوپ "Unit" دارد.
            var scope = await _authorizationChecker.GetPermissionScopeAsync(userId.Value, resourceAttr.ResourceKey, action);

            // 4. اعتبارسنجی Scope روی داده واقعی
            bool isAllowed = IsEntityInScope(dataScopedEntity, scope, userId.Value);

            if (!isAllowed)
            {
                // پرتاب خطای سفارشی شما
                throw new UnauthorizedAccessException($"User does not have permission to {action} this resource due to data scope restrictions.");
            }
        }

        private bool IsEntityInScope(IDataScopedEntity entity, ScopeType scope, Guid userId)
        {
            switch (scope)
            {
                case ScopeType.All:
                    return true; // دسترسی به همه، پس این رکورد هم مجاز است

                case ScopeType.Self:
                    // کاربر فقط می‌تواند روی داده‌های خودش این عملیات را انجام دهد
                    return entity.OwnerPersonId == userId;

                case ScopeType.Unit:
                    // کاربر فقط می‌تواند روی داده‌های واحد سازمانی خودش عملیات کند
                    var userUnitId = _currentUserService.OrganizationUnitId;
                    return entity.OwnerOrganizationUnitId == userUnitId;

                case ScopeType.UnitAndBelow:
                    // این مورد کمی پیچیده است چون نیاز به چک کردن سلسله مراتب دارد.
                    // برای سادگی در Repository معمولا Unit فعلی را چک می‌کنند
                    // اما برای پیاده‌سازی دقیق باید کد واحد را چک کنید (که نیاز به کوئری اضافه دارد)
                    // فعلاً چک ساده:
                    var uUnitId = _currentUserService.OrganizationUnitId;
                    return entity.OwnerOrganizationUnitId == uUnitId;
                // نکته: برای پشتیبانی کامل UnitAndBelow در Write، باید Path واحد را داشته باشید.

                case ScopeType.None:
                default:
                    return false; // کلا دسترسی ندارد
            }
        }
        private void SetOwnerDefaults(TEntity entity)
        {
            
            if (entity is DataScopedEntity scopedEntity)
            {
                // فقط اگر OwnerPersonId ست نشده بود، آن را پر می‌کنیم.
                if (scopedEntity.OwnerPersonId == null || scopedEntity.OwnerPersonId == Guid.Empty)
                {
                    // فرض: سرویس CurrentUser باید این اطلاعات را داشته باشد.
                    // اگر ندارید، فعلاً فقط UserId را ست می‌کنیم.
                    // طبق چارت سازمانی شما، هر شخص یک Assignment دارد که واحدش را مشخص می‌کند.
                    var personId = _currentUserService.UserId ?? Guid.Empty;
                    // استفاده از متد SetOwner که در کلاس DataScopedEntity دارید
                    scopedEntity.SetPersonOwner(personId);
                }
                if (scopedEntity.OwnerOrganizationUnitId == null || scopedEntity.OwnerOrganizationUnitId == Guid.Empty)
                {
                    var orgUnitId = _currentUserService.OrganizationUnitId ?? Guid.Empty; // نیاز است به ICurrentUserService اضافه شود
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
