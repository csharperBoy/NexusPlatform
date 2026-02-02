using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Application.Abstractions.Security;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Authorization.Infrastructure.Services
{
    public class DataScopeProcessor : IDataScopeProcessor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider; // تغییر ۱: تزریق پروایدر
        public DataScopeProcessor(ICurrentUserService currentUserService, IServiceProvider serviceProvider)
        {
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
        }

        public async Task<IQueryable<TEntity>> ApplyScope<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            // 🚨 جلوگیری از لوپ منطقی (Runtime Loop)
            // موجودیت Permission خودش ابزار تعیین دسترسی است و نباید روی خودش فیلتر امنیتی اعمال شود
            // چون PermissionEvaluator نیاز دارد پرمیشن‌ها را بخواند تا بتواند پرمیشن‌ها را چک کند!
            if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
            {
                return query;
            }
            if (typeof(IResourcedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    Guid userId = _currentUserService.UserId ?? Guid.Empty;
                    if (userId == Guid.Empty)
                        return query.Where("EquivalentResourceId == null");

                    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionInternalService>();
                    var permissions = await permissionService.GetUserPermissionsAsync(userId);

                    var allowedResourceIds = permissions.Select(p => p.ResourceId).ToList();

                    if (!allowedResourceIds.Any())
                        return query.Where("EquivalentResourceId == null");

                    // استفاده ساده و بهینه
                    return query.Where("@0.Contains(EquivalentResourceId) || EquivalentResourceId == null", allowedResourceIds);
                }
            }
            // 1. اگر موجودیت DataScoped نیست، کاری نداشته باش
            if (!typeof(IDataScopedEntity).IsAssignableFrom(typeof(TEntity)))
                return query;

            // 2. پیدا کردن Resource Key
            var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (attribute == null)
                return query;

            var personId = _currentUserService.PersonId;
            if (personId == null) return query;

            // تغییر ۲: دریافت سرویس فقط در زمان نیاز (شکستن حلقه وابستگی)
            using (var scope = _serviceProvider.CreateScope())
            {
                var permissionChecker = scope.ServiceProvider.GetRequiredService<IAuthorizationChecker>();

                // 3. دریافت دسترسی‌های کاربر
                var scopes = await permissionChecker.GetScopeForUser(personId.Value, attribute.ResourceKey);
                if (scopes.Count() == 0) return query;
                // 4. اعمال فیلتر (کد قبلی شما)
                if (scopes.Contains(ScopeType.All)) return query;

                if (scopes.Contains(ScopeType.None)) return query.Where(x => false);

                // ترکیب شرط‌ها با OR (اگر چندین اسکوپ Allow داشته باشد)
                // نکته: در EF Core بهتر است شرط‌ها را با PredicateBuilder بسازید، اما اینجا ساده‌سازی شده است
                // چون معمولا GetScopeForUser یک اسکوپ نهایی (Max) برمی‌گرداند، اما اگر لیست است:

                // اگر فقط یک اسکوپ باشد (حالت معمول)
                var maxScope = scopes.Max(); // فرض بر اینکه Enum مرتب شده است

                if (maxScope == ScopeType.Unit)
                {
                    var userUnitId = _currentUserService.OrganizationUnitId;
                    return query.Where(e => ((IDataScopedEntity)e).OwnerOrganizationUnitId == userUnitId);
                }
                if (maxScope == ScopeType.Self)
                {
                    return query.Where(e => ((IDataScopedEntity)e).OwnerPersonId == personId);
                }
            }

            return query;
        }
    }
}
