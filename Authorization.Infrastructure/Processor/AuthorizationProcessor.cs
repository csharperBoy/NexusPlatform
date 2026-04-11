using Authorization.Application.DTOs.Permissions;
using Authorization.Domain.Entities;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions.Authorization.Processor;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Context;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Processor
{
    public class AuthorizationProcessor : IAuthorizationProcessor
    {
        private readonly UserDataContext _userDataContext;
        private readonly ICachePublicService _cache;
        private readonly ILogger<AuthorizationProcessor> _logger;

        public AuthorizationProcessor(UserDataContext userDataContext, ICachePublicService cache, ILogger<AuthorizationProcessor> logger)
        {
            _userDataContext = userDataContext;
            _cache = cache;
            _logger = logger;
        }
        public async Task<bool> CheckAccessAsync(string resourceKey, string action)
        {
            //return true;

            Guid userId = _userDataContext.UserId;
            var cacheKey = $"auth:access:{userId}:{resourceKey}:{action}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<bool?>(cacheKey);
                if (cached.HasValue)
                {
                    _logger.LogDebug("Cache hit for access check: {Key}", cacheKey);
                    return cached.Value;
                }

                // ارزیابی دسترسی
                var hasAccess = await HasPermissionAsync(userId, resourceKey, action);

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, hasAccess, TimeSpan.FromMinutes(10));

                _logger.LogInformation(
                    "Access check for user {UserId} to {Resource}:{Action} = {Result}",
                    userId, resourceKey, action, hasAccess ? "GRANTED" : "DENIED");

                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in access check for user {UserId} to {Resource}:{Action}",
                    userId, resourceKey, action);
                return false; // Fail secure
            }
        }

        private async Task<bool> HasPermissionAsync(Guid userId, string resourceKey, string action)
        {
            try
            {
                var effectivePermission = await EvaluateUserPermissionsAsync(userId, resourceKey);

                var result = action.ToLower() switch
                {
                    "view" => effectivePermission.CanView,
                    "create" => effectivePermission.CanCreate,
                    "edit" => effectivePermission.CanEdit,
                    "delete" => effectivePermission.CanDelete,
                    _ => effectivePermission.CanView // پیش‌فرض
                };

                _logger.LogDebug(
                    "Permission check for user {UserId} to {Resource}:{Action} = {Result}",
                    userId, resourceKey, action, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in permission check for user {UserId} to {Resource}:{Action}",
                    userId, resourceKey, action);
                return false;
            }
        }

        private async Task<EffectivePermissionDto> EvaluateUserPermissionsAsync(Guid userId, string resourceKey)
        {
            var cacheKey = $"auth:permissions:{userId}:{resourceKey}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<EffectivePermissionDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for permissions: {Key}", cacheKey);
                    return cached;
                }

                // دریافت تمام دسترسی‌های فعال
                var activePermissionsSpec = new ActivePermissionsSpec();
                //var allPermissions = await _permissionSpecRepository.ListBySpecAsync(activePermissionsSpec);
                var allPermissions = _userDataContext.Permissions;

                Guid? personId = _userDataContext.PersonId;
                List<Guid>? positionId = _userDataContext.PositionIds?.ToList();
                List<Guid>? allUserRoles = _userDataContext.RoleIds?.ToList();
                // فیلتر دسترسی‌های مربوط به کاربر و منبع
                var userPermissions = allPermissions
                    .Where(p => (p.AppliesTo(AssigneeType.User, userId)
                                    || p.AppliesTo(AssigneeType.Role, allUserRoles)
                                    || (personId != null && p.AppliesTo(AssigneeType.Person, (Guid)personId))
                                    || (positionId != null && p.AppliesTo(AssigneeType.Position, positionId)))
                                && p.ResourceKey == resourceKey)
                    .ToList();

                // محاسبه دسترسی مؤثر
                var effectivePermission = CalculateEffectivePermission(userPermissions, resourceKey);

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, effectivePermission, TimeSpan.FromMinutes(10));

                _logger.LogDebug(
                    "Evaluated permissions for user {UserId} to resource {Resource}: {View}/{Create}/{Edit}/{Delete}",
                    userId, resourceKey, effectivePermission.CanView, effectivePermission.CanCreate,
                    effectivePermission.CanEdit, effectivePermission.CanDelete);

                return effectivePermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error evaluating permissions for user {UserId} to resource {Resource}",
                    userId, resourceKey);
                throw;
            }
        }
        private EffectivePermissionDto CalculateEffectivePermission(List<PermissionDto> permissions, string resourceKey)
        {
            if (!permissions.Any())
            {
                return new EffectivePermissionDto
                {
                    ResourceKey = resourceKey,
                    CanView = false,
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false,
                    EvaluatedAt = DateTime.UtcNow,
                    PermissionCount = 0,
                    HasExplicitDeny = false
                };
            }

            // محاسبه دسترسی‌های پایه
            var canView = permissions.Any(p => p.Effect == PermissionEffect.allow && (p.Action == PermissionAction.View || p.Action == PermissionAction.Full));
            var canCreate = permissions.Any(p => p.Effect == PermissionEffect.allow && (p.Action == PermissionAction.Create || p.Action == PermissionAction.Full));
            var canEdit = permissions.Any(p => p.Effect == PermissionEffect.allow && (p.Action == PermissionAction.Edit || p.Action == PermissionAction.Full));
            var canDelete = permissions.Any(p => p.Effect == PermissionEffect.allow && (p.Action == PermissionAction.Delete || p.Action == PermissionAction.Full));

            // اعمال منطق deny - اگر حتی یک deny وجود داشته باشد
            var denyView = permissions.Any(p => p.Effect != PermissionEffect.allow && p.Action == PermissionAction.View);
            var denyCreate = permissions.Any(p => p.Effect != PermissionEffect.allow && p.Action == PermissionAction.Create);
            var denyEdit = permissions.Any(p => p.Effect != PermissionEffect.allow && p.Action == PermissionAction.Edit);
            var denyDelete = permissions.Any(p => p.Effect != PermissionEffect.allow && p.Action == PermissionAction.Delete);

            // اعمال deny rules
            if (denyView) canView = false;
            if (denyCreate) canCreate = false;
            if (denyEdit) canEdit = false;
            if (denyDelete) canDelete = false;

            return new EffectivePermissionDto
            {
                ResourceKey = resourceKey,
                CanView = canView,
                CanCreate = canCreate,
                CanEdit = canEdit,
                CanDelete = canDelete,
                EvaluatedAt = DateTime.UtcNow,
                PermissionCount = permissions.Count

            };
        }
    }
}
