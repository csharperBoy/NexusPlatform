using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Services;
using Core.Application.Abstractions.Authorization.Processor;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Context;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Processor
{
    public class RowLevelSecurityProcessor<TEntity> : IRowLevelSecurityProcessor<TEntity>
        where TEntity : class
    {
        ICachePublicService _cacheService;
        ILogger<RowLevelSecurityProcessor<TEntity>> _logger;
        private readonly UserDataContext _scope;

        public async Task SetOwnerDefaults(TEntity entity)
        {
            if (entity is IOwnerableEntity scopedEntity)
            {
                if (scopedEntity.OwnerPersonId == null || scopedEntity.OwnerPersonId == Guid.Empty)
                {
                    var personId = _scope.PersonId ?? Guid.Empty;
                    scopedEntity.SetPersonOwner(personId);
                }
                if (scopedEntity.OwnerOrganizationUnitId == null || scopedEntity.OwnerOrganizationUnitId == Guid.Empty)
                {
                    scopedEntity.SetOrganizationUnitOwner(_scope.OrganizationUnitIds?.FirstOrDefault() ?? Guid.Empty);
                }
                if (scopedEntity.OwnerPositionId == null || scopedEntity.OwnerPositionId == Guid.Empty)
                {
                    Guid? positionId = _scope.PositionIds?.FirstOrDefault();
                    scopedEntity.SetPositionOwner(positionId ?? Guid.Empty);
                }
                if (scopedEntity.OwnerUserId == null || scopedEntity.OwnerUserId == Guid.Empty)
                {
                    scopedEntity.SetUserOwner(_scope.UserId);
                }
            }

        }

        public RowLevelSecurityProcessor(ICachePublicService cacheService , ILogger<RowLevelSecurityProcessor<TEntity>> logger, UserDataContext scope)
        {
            _cacheService = cacheService;
            _logger = logger;
            _scope = scope;
        }
        public async Task<IQueryable<TEntity>> ApplyFilter(IQueryable<TEntity> query)
        {
            try
            {
                return query;
                // 🚨 جلوگیری از لوپ منطقی
                if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
                    return query;

                List<PermissionDto> allPermissions = _scope.Permissions.ToList();


                var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
                if (attribute == null)
                    return query;

                // بخش IDataScopedEntity
                if (typeof(IOwnerableEntity).IsAssignableFrom(typeof(TEntity)))
                    query =await ApplyScope(query);

                if (typeof(IHasPermissionRuleEntity).IsAssignableFrom(typeof(TEntity)))
                    query = await ApplyPermissionRule(query);
                return query;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task CheckPermissionAsync(TEntity entity, PermissionAction action)
        {
            return;
            // تغییر ۲: جلوگیری از لوپ منطقی (بسیار مهم)
            // اگر داریم روی جدول Permission یا Resource عملیات انجام می‌دهیم، نباید چک کنیم
            // چون خود AuthorizationService برای چک کردن نیاز به خواندن اینها دارد.
             if (typeof(TEntity) == typeof(Permission) ||
                 typeof(TEntity) == typeof(Resource) ) // UserSession هم در زنجیره لاگین است
             {
                 return;
             }
            List<PermissionDto> allPermissions = _scope.Permissions.ToList();

            var resourceAttr = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (resourceAttr == null) return;
            if (resourceAttr.ResourceKey == "audit.auditlog" && action == PermissionAction.Create) return;

            var userId = _scope.UserId;
            if (userId == null) return;



            if (entity is IOwnerableEntity dataScopedEntity)
            {
                // تغییر ۳: دریافت سرویس فقط در لحظه نیاز (Lazy Resolution)
                // این کار باعث می‌شود در لحظه ساخت Repository، نیازی به ساخت AuthorizationService نباشد
                // و Circular Dependency در استارتاپ حل شود.
                //var authorizationChecker = _serviceProvider.GetRequiredService<IAuthorizationChecker>();

                var scope = await GetScopeForUser(allPermissions, resourceAttr.ResourceKey, action);

                bool isAllowed = await IsEntityInScope(dataScopedEntity, scope, userId);

                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException($"User does not have permission to {action} this resource({resourceAttr.ResourceKey}) due to data scope restrictions.");
                }

            }
        }

        private async Task<IQueryable<TEntity>> ApplyScope(IQueryable<TEntity> query)
        {
            try
            {

                // 🚨 جلوگیری از لوپ منطقی
                if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
                    return query;

                List<PermissionDto> allPermissions = _scope.Permissions.ToList();
                
               

                // بخش IDataScopedEntity
                if (!typeof(IOwnerableEntity).IsAssignableFrom(typeof(TEntity)))
                    return query;

                var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
                if (attribute == null)
                    return query;



                var scope = await GetScopeForUser(allPermissions, attribute.ResourceKey);

                if (scope == null)
                    return query.Where("false");

                if (scope == ScopeType.All)
                    return query;

                if (scope == ScopeType.None)
                    return query.Where("false");

                
                if (scope == ScopeType.Unit)
                {
                    return _scope.OrganizationUnitIds?.Count() > 0
                        ? query.Where("OwnerOrganizationUnitId in ( @0 )", _scope.OrganizationUnitIds)
                        : query.Where("false");
                }

                if (scope == ScopeType.Self)
                {
                    return query.Where("OwnerPersonId == @0", _scope.PersonId);
                }


                return query.Where("false");

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private async Task<IQueryable<TEntity>> ApplyPermissionRule(IQueryable<TEntity> query)
        {
            try
            {

                return query;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private async Task<ScopeType> GetScopeForUser(List<PermissionDto> allPermissions, string resourceKey, PermissionAction action = PermissionAction.View)
        {
            try
            {
                // کلید کش شامل اکشن هم می‌شود
                var cacheKey = $"auth:scope:{_scope.UserId}:{resourceKey}:{action}";

                // 1. بررسی کش
                
                var cached = await _cacheService.GetAsync<ScopeType?>(cacheKey);
                if (cached.HasValue)
                {
                    _logger.LogDebug("Cache hit for scope check: {Key} -> {Scope}", cacheKey, cached.Value);
                    return cached.Value;
                }

                ScopeType scope = CalculateScopeFromList(allPermissions.Where(p=>p.ResourceKey.ToLower() == resourceKey.ToLower() && 
                (p.Action == PermissionAction.Full || p.Action == action)
                
                ));
               

                // 3. ذخیره در کش
                await _cacheService.SetAsync(cacheKey, scope, TimeSpan.FromMinutes(10));

                return scope;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private ScopeType CalculateScopeFromList(IEnumerable<PermissionDto> permissions)
        {
            if (permissions == null || !permissions.Any()) return ScopeType.None;

            // اولویت ۱: شخص
            var personPerm = permissions.FirstOrDefault(p => p.AssigneeType == AssigneeType.Person);
            if (personPerm != null)
            {
                return personPerm.Effect == PermissionEffect.Deny ? ScopeType.None : personPerm.Scopes.FirstOrDefault().scope;
            }

            // اولویت ۲: وجود Deny در نقش/پست
            if (permissions.Any(p => p.Effect == PermissionEffect.Deny))
            {
                return ScopeType.None;
            }

            // اولویت ۳: Max Scope
            return permissions
                .Where(p => p.Effect == PermissionEffect.allow)
                .Select(p => p.Scopes.FirstOrDefault().scope)
                .DefaultIfEmpty(ScopeType.None)
                .Max();
        }
     
       private async Task<bool> IsEntityInScope(IOwnerableEntity entity, ScopeType scope, Guid userId)
        {
            switch (scope)
            {
                case ScopeType.All:
                    return true;
                case ScopeType.Account:
                    return entity.OwnerUserId == userId;
                case ScopeType.Self:
                    var userPersonId = _scope.PersonId;
                    return entity.OwnerPersonId == userId;
                case ScopeType.Unit:
                    return _scope.OrganizationUnitIds.Any(a=>a ==  entity.OwnerOrganizationUnitId ) ;
                case ScopeType.UnitAndBelow:
                    return _scope.OrganizationUnitIds.Any(a=>a == entity.OwnerOrganizationUnitId );
                case ScopeType.None:
                default:
                    return false;
            }
        }

    }
}
