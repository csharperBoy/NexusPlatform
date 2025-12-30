using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
    public class PermissionEvaluator : IPermissionEvaluator
    {
        private readonly ISpecificationRepository<Permission, Guid> _permissionSpecRepository;
        private readonly ILogger<PermissionEvaluator> _logger;
        private readonly ICacheService _cache;

        public PermissionEvaluator(
            ISpecificationRepository<Permission, Guid> permissionSpecRepository,
            ILogger<PermissionEvaluator> logger,
            ICacheService cache)
        {
            _permissionSpecRepository = permissionSpecRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<EffectivePermissionDto> EvaluateUserPermissionsAsync(Guid userId, string resourceKey)
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
                var allPermissions = await _permissionSpecRepository.ListBySpecAsync(activePermissionsSpec);

                // فیلتر دسترسی‌های مربوط به کاربر و منبع
                var userPermissions = allPermissions
                    .Where(p => p.AppliesTo(AssigneeType.Person, userId) &&
                               p.Resource.Key == resourceKey)
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

        public async Task<IReadOnlyList<EffectivePermissionDto>> EvaluateAllUserPermissionsAsync(Guid userId)
        {
            var cacheKey = $"auth:allpermissions:{userId}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<IReadOnlyList<EffectivePermissionDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for all permissions: {Key}", cacheKey);
                    return cached;
                }

                // دریافت تمام دسترسی‌های فعال
                var activePermissionsSpec = new ActivePermissionsSpec();
                var allPermissions = await _permissionSpecRepository.ListBySpecAsync(activePermissionsSpec);

                var userPermissions = allPermissions
                    .Where(p => p.AppliesTo(AssigneeType.Person, userId))
                    .GroupBy(p => p.Resource.Key)
                    .Select(g => CalculateEffectivePermission(g.ToList(), g.Key))
                    .ToList();

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, userPermissions, TimeSpan.FromMinutes(5));

                _logger.LogInformation(
                    "Evaluated {Count} permissions for user {UserId}",
                    userPermissions.Count, userId);

                return userPermissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating all permissions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string resourceKey, string action)
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

        private EffectivePermissionDto CalculateEffectivePermission(List<Permission> permissions, string resourceKey)
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
            var canView = permissions.Any(p => p.Type == PermissionType.allow && p.Action == PermissionAction.View);
            var canCreate = permissions.Any(p => p.Type == PermissionType.allow && p.Action == PermissionAction.Create);
            var canEdit = permissions.Any(p => p.Type == PermissionType.allow && p.Action == PermissionAction.Edit);
            var canDelete = permissions.Any(p => p.Type == PermissionType.allow && p.Action == PermissionAction.Delete);

            // اعمال منطق deny - اگر حتی یک deny وجود داشته باشد
            var denyView = permissions.Any(p => p.Type != PermissionType.allow && p.Action == PermissionAction.View);
            var denyCreate = permissions.Any(p => p.Type != PermissionType.allow && p.Action == PermissionAction.Create);
            var denyEdit = permissions.Any(p => p.Type != PermissionType.allow && p.Action == PermissionAction.Edit);
            var denyDelete = permissions.Any(p => p.Type != PermissionType.allow && p.Action == PermissionAction.Delete);

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