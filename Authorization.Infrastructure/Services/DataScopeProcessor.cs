using Authorization.Domain.Enums;
using Core.Application.Abstractions.Security;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class DataScopeProcessor : IDataScopeProcessor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthorizationChecker _permissionChecker; // سرویسی که پرمیشن‌ها را می‌خواند

        public DataScopeProcessor(ICurrentUserService currentUserService, IAuthorizationChecker permissionChecker)
        {
            _currentUserService = currentUserService;
            _permissionChecker = permissionChecker;
        }

        public async Task<IQueryable<TEntity>> ApplyScope<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            // 1. اگر موجودیت DataScoped نیست، کاری نداشته باش
            if (!typeof(IDataScopedEntity).IsAssignableFrom(typeof(TEntity)))
                return query;

            // 2. پیدا کردن Resource Key از روی Attribute
            var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (attribute == null)
                return query; // یا خطا پرتاب کنید، بسته به سیاست امنیتی

            var userId = _currentUserService.UserId;
            if (userId == null) return query; // اگر کاربر لاگین نیست (مثلا جاب پس‌زمینه)، شاید بخواهید کلا دسترسی ندهید یا کامل بدهید.

            // 3. دریافت دسترسی‌های کاربر برای این Resource
            // فرض: این متد لیست دسترسی‌های کاربر روی این ریسورس را برمی‌گرداند (بیشترین اسکوپ محاسبه شده)
            var scope =await _permissionChecker.GetScopeForUser(userId.Value, attribute.ResourceKey);

            // 4. اعمال فیلتر بر اساس Scope
            // نکته: اینجا باید با Expression Trees یا Dynamic LINQ کار کرد چون TEntity جنریک است
            // اما چون ما اینترفیس IDataScopedEntity را داریم، می‌توانیم کست کنیم (با محدودیت‌هایی در EF)
            // بهترین راه برای EF Core استفاده از شرط‌های داینامیک است.

            // پیاده‌سازی ساده‌شده (برای درک مطلب):
            if (scope.Any(x=>x ==  ScopeType.All)) return query;


            if (scope.Any(x => x == ScopeType.Unit))
            {
                // نیاز دارید UnitId کاربر را هم داشته باشید
                var userUnitId = _currentUserService.OrganizationUnitId;
                return query.Where(e => ((IDataScopedEntity)e).OwnerOrganizationUnitId == userUnitId);
            }
            if (scope.Any(x => x == ScopeType.Self))
            {
                // query.Where(e => e.OwnerPersonId == userId)
                return query.Where(e => ((IDataScopedEntity)e).OwnerPersonId == userId);
            }

            if (scope.Any(x => x == ScopeType.None))
            {
                // برگرداندن لیست خالی
                return query.Where(x => false);
            }

            // سایر اسکوپ‌ها مثل UnitAndBelow نیاز به لاجیک پیچیده‌تری دارند (بررسی Path)

            return query;
        }

       
    }
}
