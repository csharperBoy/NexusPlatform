using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Domain.Interfaces;
using Core.Shared.DTOs.Identity;
using Core.Shared.Enums.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Core.Shared.Enums;
namespace Authorization.Infrastructure.Services
{
    public class PermissionService : IPermissionInternalService
    {
        private readonly IRepository<AuthorizationDbContext, Permission, Guid> _permissionRepository;
        private readonly ISpecificationRepository<Permission, Guid> _permissionSpecRepository;
        private readonly ISpecificationRepository<Resource, Guid> _resourceSpecRepository;
        private readonly IRepository<AuthorizationDbContext, Resource, Guid> _resourceRepository;
        private readonly IUnitOfWork<AuthorizationDbContext> _unitOfWork;
        private readonly ILogger<PermissionService> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly ICacheService _cache;
        private readonly IRolePublicService _roleService;
        private readonly IUserPublicService _userService;

        public PermissionService(
            IRepository<AuthorizationDbContext, Permission, Guid> permissionRepository,
            ISpecificationRepository<Permission, Guid> permissionSpecRepository,
            IRepository<AuthorizationDbContext, Resource, Guid> resourceRepository,
            ISpecificationRepository<Resource, Guid> resourceSpecRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<PermissionService> logger,
            ICurrentUserService currentUser, IRolePublicService roleService, IUserPublicService userService,
            ICacheService cache)
        {
            _permissionRepository = permissionRepository;
            _permissionSpecRepository = permissionSpecRepository;
            _resourceRepository = resourceRepository;
            _resourceSpecRepository = resourceSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache;
            _roleService = roleService;
            _userService = userService;
        }

        public async Task<Guid> AssignPermissionAsync(AssignPermissionCommand command)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission assignment for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId);

                // اعتبارسنجی وجود Resource
                var resource = await _resourceRepository.GetByIdAsync(command.ResourceId);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {command.ResourceId} not found");
                }

                // بررسی تداخل
                var hasConflict = await CheckPermissionConflictAsync(command);
                if (hasConflict)
                {
                    throw new InvalidOperationException(
                        $"Permission conflict detected for {command.AssigneeType}:{command.AssigneeId} " +
                        $"on resource {command.ResourceId} with action {command.Action}");
                }

                // ایجاد Permission جدید
                var permission = new Permission(
                    command.ResourceId,
                    command.AssigneeType,
                    command.AssigneeId,
                    command.Action, command.scope, command.specificScopeId, command.type,
                    command.EffectiveFrom,
                    command.ExpiresAt,
                    command.Description,
                    createdBy: _currentUser.UserId?.ToString() ?? "system"
                );

                // ذخیره در Repository
                await _permissionRepository.AddAsync(permission);

                // انتشار ایونت
                permission.AddDomainEvent(new PermissionChangedEvent(command.AssigneeId, command.ResourceId));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش‌های مرتبط
                await InvalidatePermissionCachesAsync(command.AssigneeId, command.ResourceId);

                _logger.LogInformation(
                    "Permission assigned successfully: {PermissionId} for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    permission.Id, command.AssigneeType, command.AssigneeId, command.ResourceId);

                return permission.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign permission for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId);
                throw;
            }
        }

        public async Task RevokePermissionAsync(Guid permissionId)
        {
            try
            {
                _logger.LogInformation("Starting permission revocation: {PermissionId}", permissionId);

                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    throw new ArgumentException($"Permission with ID {permissionId} not found");
                }

                var assigneeId = permission.AssigneeId;
                var resourceId = permission.ResourceId;

                // غیرفعال کردن Permission
                permission.Deactivate();

                // انتشار ایونت
                permission.AddDomainEvent(new PermissionChangedEvent(assigneeId, resourceId));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidatePermissionCachesAsync(assigneeId, resourceId);

                _logger.LogInformation("Permission revoked successfully: {PermissionId}", permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke permission: {PermissionId}", permissionId);
                throw;
            }
        }

        public async Task TogglePermissionAsync(Guid permissionId, bool isActive)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission toggle: {PermissionId} to {IsActive}",
                    permissionId, isActive);

                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    throw new ArgumentException($"Permission with ID {permissionId} not found");
                }

                var assigneeId = permission.AssigneeId;
                var resourceId = permission.ResourceId;

                if (isActive)
                {
                    permission.Activate();
                }
                else
                {
                    permission.Deactivate();
                }

                // انتشار ایونت
                permission.AddDomainEvent(new PermissionChangedEvent(assigneeId, resourceId));

                await _unitOfWork.SaveChangesAsync();
                await InvalidatePermissionCachesAsync(assigneeId, resourceId);

                _logger.LogInformation(
                    "Permission toggled successfully: {PermissionId} to {IsActive}",
                    permissionId, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to toggle permission: {PermissionId} to {IsActive}",
                    permissionId, isActive);
                throw;
            }
        }

        public async Task<PermissionDto> GetPermissionAsync(Guid permissionId)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    _logger.LogWarning("Permission not found: {PermissionId}", permissionId);
                    return null;
                }

                var dto = MapToDto(permission);
                _logger.LogDebug("Retrieved permission: {PermissionId}", permissionId);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission: {PermissionId}", permissionId);
                throw;
            }
        }

        public async Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                var spec = new PermissionsByUserSpec(userId);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                var dtos = permissions.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} permissions for user {UserId}",
                    dtos.Count, userId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckPermissionConflictAsync(AssignPermissionCommand command)
        {
            try
            {
                var conflictSpec = new PermissionConflictSpec(
                    command.AssigneeType,
                    command.AssigneeId,
                    command.ResourceId,
                    command.Action);

                var existingPermissions = await _permissionSpecRepository.ListBySpecAsync(conflictSpec);
                var hasConflict = existingPermissions.Any();

                _logger.LogDebug(
                    "Permission conflict check for {AssigneeType}:{AssigneeId} on {ResourceId}:{Action} = {HasConflict}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId, command.Action, hasConflict);

                return hasConflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking permission conflict for {AssigneeType}:{AssigneeId} on {ResourceId}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId);
                throw;
            }
        }

        public async Task ResolvePermissionConflictsAsync(Guid userId, string resourceKey)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission conflict resolution for user {UserId} on resource {Resource}",
                    userId, resourceKey);

                // دریافت Resource برای گرفتن ID
                var resource = await _resourceSpecRepository.GetBySpecAsync(new ResourceByKeySpec(resourceKey));
                if (resource == null)
                {
                    _logger.LogWarning("Resource not found: {ResourceKey}", resourceKey);
                    return;
                }

                // دریافت تمام دسترسی‌های کاربر برای این منبع
                var userPermissionsSpec = new PermissionsByUserSpec(userId);
                var allUserPermissions = await _permissionSpecRepository.ListBySpecAsync(userPermissionsSpec);

                var userPermissions = allUserPermissions
                    .Where(p => p.ResourceId == resource.Id && p.IsValid)
                    .ToList();

                if (!userPermissions.Any())
                {
                    _logger.LogInformation("No permissions found for conflict resolution");
                    return;
                }

                // گروه‌بندی بر اساس Action و اعمال منطق اولویت
                var groupedByAction = userPermissions.GroupBy(p => p.Action);

                foreach (var group in groupedByAction)
                {
                    var permissions = group.ToList();
                    if (permissions.Count <= 1) continue;



                    _logger.LogInformation(
                        "Resolved conflicts for action {Action} on resource {Resource}, keeping permission {PermissionId}",
                        group.Key, resourceKey);
                }

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidatePermissionCachesAsync(userId, resource.Id);

                _logger.LogInformation(
                    "Permission conflicts resolved successfully for user {UserId} on resource {Resource}",
                    userId, resourceKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to resolve permission conflicts for user {UserId} on resource {Resource}",
                    userId, resourceKey);
                throw;
            }
        }


        private async Task InvalidatePermissionCachesAsync(Guid assigneeId, Guid resourceId)
        {
            try
            {
                await _cache.RemoveByPatternAsync($"auth:permissions:{assigneeId}:*");
                await _cache.RemoveByPatternAsync($"auth:access:{assigneeId}:*");
                await _cache.RemoveByPatternAsync($"auth:useraccess:{assigneeId}");
                await _cache.RemoveByPatternAsync($"auth:allpermissions:{assigneeId}");

                _logger.LogDebug("Invalidated permission caches for assignee {AssigneeId}", assigneeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating permission caches for assignee {AssigneeId}", assigneeId);
            }
        }

        private PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                ResourceKey = permission.Resource.Key,
                ResourceId = permission.ResourceId,
                AssigneeType = permission.AssigneeType,
                AssigneeId = permission.AssigneeId,
                Action = permission.Action,
                Scope = permission.Scope,
                SpecificScopeId = permission.SpecificScopeId,
                Type = permission.Type,
                IsActive = permission.IsActive,
                EffectiveFrom = permission.EffectiveFrom,
                ExpiresAt = permission.ExpiresAt,
                Description = permission.Description,
            };
        }

        public async Task SeedRolePermissionsAsync(List<PermissionDefinition> permissions, CancellationToken cancellationToken = default)
        {
            var initializeruser = await _userService.GetUserId("intitializer");
            foreach (var permissionDefinition in permissions)
            {
                ResourceByKeySpec specByKey = new ResourceByKeySpec(permissionDefinition.ResourceKey);
                var specRet = await _resourceSpecRepository.FindBySpecAsync(specByKey);
                Permission permission = new Permission(
                    specRet.Items.FirstOrDefault().Id, permissionDefinition.AssignType.ToEnumOrDefault(AssigneeType.Role),
                    permissionDefinition.AssignId,
                    permissionDefinition.Action.ToEnumOrDefault(Core.Shared.Enums.Authorization.PermissionAction.View),
                    permissionDefinition.Scope.ToEnumOrDefault(ScopeType.Self),
                    null,
                    permissionDefinition.Type.ToEnumOrDefault(PermissionType.allow)
                    );

                permission.SetUserOwner(initializeruser);
                if (!await IsExist(permission))
                    await _permissionRepository.AddAsync(permission);

            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> IsExist(Permission permission)
        {
            try
            {
                return await _permissionRepository.ExistsAsync(
                    p => p.ResourceId == permission.ResourceId &&
                    p.Scope == permission.Scope &&
                    p.SpecificScopeId == permission.SpecificScopeId && // همیشه مقدار دارد
                    p.Action == permission.Action &&
                    p.AssigneeType == permission.AssigneeType &&
                    p.AssigneeId == permission.AssigneeId &&
                    p.Type == permission.Type
         );
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async     Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(List<Guid>? roleIds)
        {
            try
            {
                var spec = new PermissionsByRolesSpec(roleIds);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                var dtos = permissions.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} permissions for roles {roleIds}",
                    dtos.Count, roleIds);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for roles {roleIds}", roleIds);
                throw;
            }
        }

        public async Task<IReadOnlyList<PermissionDto>> GetPersonPermissionsAsync(Guid? personId)
        {
            try
            {
                if (personId == null)
                    return null;
                var spec = new PermissionsByPersonSpec((Guid)personId);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                var dtos = permissions.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} permissions for person {personId}",
                    dtos.Count, personId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for person {personId}", personId);
                throw;
            }
        }

        public async Task<IReadOnlyList<PermissionDto>> GetPositionPermissionsAsync(List<Guid>? positionIds)
        {
            try
            {
                var spec = new PermissionsByPositionsSpec(positionIds);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                var dtos = permissions.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} permissions for Position {UsepositionIdsrId}",
                    dtos.Count, positionIds);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for position {positionIds}", positionIds);
                throw;
            }
        }
    }
}