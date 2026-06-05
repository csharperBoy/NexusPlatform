using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.DTOs.Resource;
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
using Core.Infrastructure.Hosted;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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
        private readonly ApplicationLifetimeTracker _lifetimeTracker;
        private readonly IResourceMetadataProvider _metadataProvider;
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

        public RowLevelSecurityProcessor(ICachePublicService cacheService ,
            ILogger<RowLevelSecurityProcessor<TEntity>> logger,
            UserDataContext scope,
            ApplicationLifetimeTracker lifetimeTracker,
            IResourceMetadataProvider metadataProvider)
        {
            _cacheService = cacheService;
            _logger = logger;
            _scope = scope;
            _lifetimeTracker = lifetimeTracker;
            _metadataProvider = metadataProvider;
        }
        public async Task<IQueryable<TEntity>> ApplyFilter(IQueryable<TEntity> query)
        {
            try
            {
                if (!_lifetimeTracker.IsStarted)
                {
                    // اگر در حال اجرای اولیه پروژه بود اعمال نشود
                    return query;
                }
                // 🚨 جلوگیری از لوپ منطقی
               // if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
               //     return query;

                List<PermissionDto> allPermissions = _scope.Permissions.ToList();


                var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
                if (attribute == null)
                    return query;

                string resourceKey = attribute.ResourceKey.ToLower() ?? typeof(TEntity).Name;
                var metadata = _metadataProvider.GetMetadata(resourceKey);
                if (metadata == null)
                    return query;

                // بخش IDataScopedEntity
                //if (typeof(IOwnerableEntity).IsAssignableFrom(typeof(TEntity)))
                if (metadata.useScope)
                {
                    query = await ApplyScope(query);
                }

                //if (typeof(IHasPermissionRuleEntity).IsAssignableFrom(typeof(TEntity)))
                if (metadata.useDynamicFilter)
                {
                    query = await ApplyPermissionRule(query);
                }

                return query;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task CheckPermissionAsync(TEntity entity, PermissionAction action)
        {
            if (!_lifetimeTracker.IsStarted)
                return;

            // جلوگیری از لوپ منطقی
            if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
                return;

            var resourceAttr = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (resourceAttr == null) return;

            // استثنا برای AuditLog
            if (resourceAttr.ResourceKey == "audit.auditlog" && action == PermissionAction.Create) return;

            var userId = _scope.UserId;
            if (userId == null) return;

            var allPermissions = _scope.Permissions.ToList();
            string resourceKey = resourceAttr.ResourceKey ?? typeof(TEntity).Name;

            // ۱. بررسی دسترسی پایه (وجود حداقل یک مجوز Allow برای این اکشن)
            var hasAllowPermission = allPermissions.Any(p =>
                p.ResourceKey.Equals(resourceKey, StringComparison.OrdinalIgnoreCase) &&
                (p.Action == PermissionAction.Full || p.Action == action) &&
                p.Effect == PermissionEffect.allow);

            if (!hasAllowPermission)
            {
                throw new UnauthorizedAccessException($"User does not have {action} permission on resource {resourceKey}.");
            }

            // ۲. اگر موجودیت مالکیت‌پذیر است و متادیتا useScope دارد، محدوده را بررسی کن
            var metadata = _metadataProvider.GetMetadata(resourceKey);
            if (metadata?.useScope == true 
                //&& entity is IOwnerableEntity ownerableEntity
                )
            {
                var scope = await GetScopeForUser(allPermissions, resourceKey, action);
                bool isAllowed = await IsEntityInScope((IOwnerableEntity)entity, scope, userId);
                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException($"User does not have permission to {action} this specific record due to data scope restrictions.");
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
        #region permission rule
        private async Task<List<PermissionRuleDto>> GetEffectiveRulesForResource(string resourceKey)
        {
            var allPermissions = _scope.Permissions;
            var relevantPermissions = allPermissions
                .Where(p => p.ResourceKey.Equals(resourceKey, StringComparison.OrdinalIgnoreCase) &&
                            p.Effect == PermissionEffect.allow &&
                            p.rules != null && p.rules.Any())
                .ToList();

            return relevantPermissions.SelectMany(p => p.rules).ToList();
        }
        private async Task<IQueryable<TEntity>> ApplyPermissionRule(IQueryable<TEntity> query)
        {
            var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (attribute == null) return query;

            string resourceKey = attribute.ResourceKey.ToLower() ?? typeof(TEntity).Name;
            var metadata = _metadataProvider.GetMetadata(resourceKey);
            if (metadata == null || !metadata.useDynamicFilter) return query;

            var rules = await GetEffectiveRulesForResource(resourceKey);
            if (!rules.Any()) return query;

            var filterExpression = BuildFilterExpression<TEntity>(rules, metadata);
            if (filterExpression != null)
                query = query.Where(filterExpression);

            return query;
        }
        private Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>(
    List<PermissionRuleDto> rules, ResourceMetadataDto metadata)
        {
            if (!rules.Any()) return null;

            // گروه‌بندی بر اساس GroupOrder (اختیاری)
            var groups = rules.GroupBy(r => r.GroupOrder).OrderBy(g => g.Key);
            Expression<Func<TEntity, bool>>? combined = null;

            foreach (var group in groups)
            {
                Expression<Func<TEntity, bool>>? groupExpr = null;
                foreach (var rule in group)
                {
                    var singleExpr = BuildSingleRuleExpression<TEntity>(rule, metadata);
                    if (singleExpr == null) continue;
                    if (groupExpr == null)
                        groupExpr = singleExpr;
                    else
                        groupExpr = CombineExpressions(groupExpr, singleExpr, Expression.AndAlso);
                }
                if (groupExpr == null) continue;
                if (combined == null)
                    combined = groupExpr;
                else
                    combined = CombineExpressions(combined, groupExpr, Expression.OrElse);
            }
            return combined;
        }
        private Expression<Func<TEntity, bool>> BuildSingleRuleExpression<TEntity>(
    PermissionRuleDto rule, ResourceMetadataDto metadata)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression left;

            if (!string.IsNullOrEmpty(rule.JoinEntity))
            {
                var joinInfo = metadata.Joins.FirstOrDefault(j => j.NavigationName == rule.JoinEntity);
                if (joinInfo == null) return null;

                var navProp = typeof(TEntity).GetProperty(rule.JoinEntity);
                if (navProp == null) return null;
                var navigation = Expression.Property(parameter, navProp);

                // تشخیص نوع نویگیشن
                bool isCollection = IsCollectionType(navProp.PropertyType);
                Type targetType;

                if (isCollection)
                {
                    // یک به چند: دریافت نوع المان مجموعه (مانند T در ICollection<T>)
                    targetType = navProp.PropertyType.GetGenericArguments().First();
                    var targetParam = Expression.Parameter(targetType, "t");
                    var targetField = Expression.Property(targetParam, rule.FieldName);
                    var right = ConvertValueToType(rule.Value, targetField.Type);
                    var comparison = BuildComparison(targetField, right, rule.Operator);
                    if (comparison == null) return null;
                    var anyLambda = Expression.Lambda(comparison, targetParam);
                    var anyCall = Expression.Call(typeof(Enumerable), "Any", new[] { targetType }, navigation, anyLambda);
                    left = anyCall;
                }
                else
                {
                    // یک به یک: مستقیماً به property دسترسی پیدا کن
                    var targetField = Expression.Property(navigation, rule.FieldName);
                    var right = ConvertValueToType(rule.Value, targetField.Type);
                    left = BuildComparison(targetField, right, rule.Operator);
                    if (left == null) return null;
                }

                return Expression.Lambda<Func<TEntity, bool>>(left, parameter);
            }
            else
            {
                // بدون جوین (قبلاً پیاده شده)
                var property = typeof(TEntity).GetProperty(rule.FieldName);
                if (property == null) return null;
                left = Expression.Property(parameter, property);
                var right = ConvertValueToType(rule.Value, left.Type);
                left = BuildComparison(left, right, rule.Operator);
                if (left == null) return null;
                return Expression.Lambda<Func<TEntity, bool>>(left, parameter);
            }
        }

        private bool IsCollectionType(Type type)
        {
            // از string صرف نظر کن (IEnumerable<char> محسوب می‌شود ولی نباید مجموعه در نظر گرفته شود)
            if (type == typeof(string)) return false;
            return typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType;
        }
        private Expression BuildComparison(Expression left, Expression right, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equal: return Expression.Equal(left, right);
                case ComparisonOperator.NotEqual: return Expression.NotEqual(left, right);
                case ComparisonOperator.GreateThan: return Expression.GreaterThan(left, right);
                case ComparisonOperator.LessThan: return Expression.LessThan(left, right);
                default: return null;
            }
        }

        private Expression ConvertValueToType(object value, Type targetType)
        {
            if (value == null)
                return Expression.Constant(null, targetType);
            var converted = Convert.ChangeType(value, targetType);
            return Expression.Constant(converted, targetType);
        }

        private Expression<Func<T, bool>> CombineExpressions<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right,
            Func<Expression, Expression, BinaryExpression> combiner)
        {
            var param = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceVisitor(left.Parameters[0], param);
            var rightVisitor = new ReplaceVisitor(right.Parameters[0], param);
            var combined = combiner(leftVisitor.Visit(left.Body), rightVisitor.Visit(right.Body));
            return Expression.Lambda<Func<T, bool>>(combined, param);
        }

        private class ReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _old;
            private readonly ParameterExpression _new;
            public ReplaceVisitor(ParameterExpression oldParam, ParameterExpression newParam) { _old = oldParam; _new = newParam; }
            protected override Expression VisitParameter(ParameterExpression node) => node == _old ? _new : base.VisitParameter(node);
        }
        #endregion
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
                .Select(p => p.Scopes != null ? p.Scopes.FirstOrDefault().scope : ScopeType.All)
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
