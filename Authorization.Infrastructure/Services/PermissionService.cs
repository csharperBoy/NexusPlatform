using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces.Processor;
using Authorization.Application.Interfaces.Service;
using Authorization.Application.Queries.Permissions;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.Identity;
using Core.Application.Context;
using Core.Domain.Interfaces;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        private readonly ICachePublicService _cache;
        private readonly UserDataContext _currentUserContext;

        private readonly IRepository<AuthorizationDbContext, Scope, Guid> _scopeRepository;
        private readonly ISpecificationRepository<Scope, Guid> _scopeSpecRepository;
        private readonly IScopeProcessor _scopeProcessor;

        private readonly IRepository<AuthorizationDbContext, PermissionRule, Guid> _permissionRuleRepository;
        private readonly ISpecificationRepository<PermissionRule, Guid> _permissionRuleSpecRepository;

        private readonly string baseCacheKey = "authorization:permission";


        public PermissionService(
            IRepository<AuthorizationDbContext, Permission, Guid> permissionRepository,
            ISpecificationRepository<Permission, Guid> permissionSpecRepository,
            IRepository<AuthorizationDbContext, Resource, Guid> resourceRepository,
            ISpecificationRepository<Resource, Guid> resourceSpecRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<PermissionService> logger,
            UserDataContext currentUserContext,
            ICachePublicService cache,
            IScopeProcessor scopeProcessor,
            IRepository<AuthorizationDbContext, Scope, Guid> scopeRepository,
            ISpecificationRepository<Scope, Guid> scopeSpecRepository,
            IRepository<AuthorizationDbContext, PermissionRule, Guid> permissionRuleRepository,
            ISpecificationRepository<PermissionRule, Guid> permissionRuleSpecRepository

         )
        {
            _permissionRepository = permissionRepository;
            _permissionSpecRepository = permissionSpecRepository;
            _resourceRepository = resourceRepository;
            _resourceSpecRepository = resourceSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserContext = currentUserContext;
            _cache = cache;
            _scopeProcessor = scopeProcessor;
            _scopeRepository = scopeRepository;
            _scopeSpecRepository = scopeSpecRepository;
            _permissionRuleRepository = permissionRuleRepository;
            _permissionRuleSpecRepository = permissionRuleSpecRepository;

        }
        #region Rule
     
        private async Task DeletePermissionRuleAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Starting permissionRule Delete : {id}", id);

                await _permissionRuleRepository.DeleteAsync(id);

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation("Permission Rule Delete successfully: {id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed Permission Rule Delete : {id}", id);
                throw;
            }
        }

        private async Task<PermissionRuleDto?> PermissionRuleGetById(Guid id)
        {
            try
            {
                var permission = await _permissionRuleRepository.GetByIdAsync(id);

                if (permission == null)
                {
                    _logger.LogWarning("PermissionRule not found: {id}", id);
                    return null;
                }

                PermissionRuleDto dto = MapToDto(permission);
                _logger.LogDebug("Retrieved permissionRule: {id}", id);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission: {id}", id);
                throw;
            }
        }

        private PermissionRuleDto MapToDto(PermissionRule model)
        {

            return new PermissionRuleDto()
            {
                Id = model.Id,
                PermissionId = model.PermissionId,
                FieldName = model.FieldName,
                GroupOrder = model.GroupOrder,
                Operator = model.Operator,
                LogicalOperator = model.LogicalOperator,
                Value = model.Value,
                JoinEntity = model.JoinEntity,
                JoinForeignKey = model.JoinForeignKey,
                JoinLocalKey = model.JoinLocalKey,
            };

        }

        private async Task<IReadOnlyList<PermissionRuleDto>> GetPermissionRules(Guid? permissionId)
        {
            var cacheKey = $"{baseCacheKey}:full";
            var cached = await _cache.GetAsync<IReadOnlyList<PermissionRuleDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for full resource tree");
                return cached;
            }
            var spec = new GetPermissionRulesSpec(permissionId);
            var permissions = await _permissionRuleSpecRepository.ListBySpecAsync(spec);


            var result = permissions.Select(MapToDto).ToList();


            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;
        }

        private async Task UpdatePermissionRuleAsync(Guid id, Guid? permissionId, string? fieldName, ComparisonOperator? comparisonOperator, string? value, LogicalOperator? logicalOperator, int? groupOrder,
             string? joinLocalKey,
            string? joinForeignKey,
            string? joinEntity)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission rule update");

                var permissionRule = await _permissionRuleRepository.GetByIdAsync(id);
                if (permissionRule == null)
                {
                    throw new ArgumentException($"Permission with ID {id} not found");
                }
                permissionRule.ApplyChange(permissionId, fieldName, comparisonOperator, value, logicalOperator, groupOrder,
                     joinLocalKey,
                    joinForeignKey,
                    joinEntity

                    );
                // انتشار ایونت
                //permissionRule.AddDomainEvent(new PermissionChangedEvent((Guid)AssigneeId, (Guid)ResourceId));
                await _permissionRuleRepository.UpdateAsync(permissionRule);
                await _unitOfWork.SaveChangesAsync();
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation("Permission rule update successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update permission rule");
                throw;
            }
        }
        private async Task AddRulesToPermission(Guid permissionId, List<PermissionRuleCreateDto>? rules)
        {
            List<PermissionRule> newList = rules.Select(r =>
                    new PermissionRule(permissionId,r.FieldName,r.Operator,r.Value,r.LogicalOperator,r.GroupOrder,r.JoinLocalKey,r.JoinForeignKey,r.JoinEntity)
            ).ToList();
            await _permissionRuleRepository.AddRangeAsync(newList);
        }

        private async Task UpdateRuleOfPermission(Guid permissionId, List<PermissionRuleCreateDto>? newRules)
        {
            var existRuleList = await GetPermissionRulesList(permissionId);
            //bool isEqual = await _scopeProcessor.compareTwoScopeList(existScopeList.Select(s => s.scope).ToList(), newScopes);
            if (!existRuleList.Select(r=>new PermissionRuleDto
            {
                PermissionId = r.PermissionId,
                FieldName=r.FieldName,
                GroupOrder=r.GroupOrder,
                JoinEntity=r.JoinEntity,
                JoinForeignKey=r.JoinForeignKey   ,
                JoinLocalKey=r.JoinLocalKey,
                LogicalOperator=r.LogicalOperator,
                Operator=r.Operator,
                Value = r.Value

            }).Equals(newRules))
            {
                await _permissionRuleRepository.RemoveRangeAsync(existRuleList);
                await AddRulesToPermission(permissionId, newRules);
            }

        }
        private async Task<List<PermissionRule>> GetPermissionRulesList(Guid permissionId)
        {
            var list = await _permissionRuleSpecRepository.ListBySpecAsync(new GetRulesByPermissionIdSpec(permissionId));
            return list.ToList();
        }
        #endregion
        #region Scope
        private async Task UpdateScopeOfPermission(Guid permissionId, List<ScopeType>? newScopes)
        {
            var existScopeList = await GetPermissionScopesList(permissionId);
            //bool isEqual = await _scopeProcessor.compareTwoScopeList(existScopeList.Select(s => s.scope).ToList(), newScopes);
            if (!existScopeList.Equals(newScopes))
            {
                await _scopeRepository.RemoveRangeAsync(existScopeList);
                await AddScopesToPermission(permissionId, newScopes);

            }

        }


        private async Task AddScopesToPermission(Guid permissionId, List<ScopeType>? scopes)
        {
            List<Scope> newList = scopes.Select(s => new Scope
            {
                scope = s,
                PermissionId = permissionId
            }).ToList();
            await _scopeRepository.AddRangeAsync(newList);
        }


        private async Task<List<Scope>> GetPermissionScopesList(Guid permissionId)
        {
            var list = await _scopeSpecRepository.ListBySpecAsync(new GetScopesByPermissionIdSpec(permissionId));
            return list.ToList();
        }
        #endregion
        public async Task<Guid> AssignPermissionAsync(
             Guid ResourceId,
        Guid AssigneeId,
        AssigneeType AssigneeType = AssigneeType.User,
        PermissionAction Action = PermissionAction.Full,
        PermissionEffect effect = PermissionEffect.allow,
        bool IsActive = true,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        string? Description = null,

        List<ScopeType>? scopes = null,
        List<PermissionRuleCreateDto>? rules = null
            )
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission assignment for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    AssigneeType, AssigneeId, ResourceId);

                // اعتبارسنجی وجود Resource
                var resource = await _resourceRepository.GetByIdAsync(ResourceId);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {ResourceId} not found");
                }

                // بررسی تداخل
                var hasConflict = await CheckPermissionConflictAsync(
                      ResourceId,
                      AssigneeId,
                      AssigneeType,
                      Action
                    );
                if (hasConflict)
                {
                    throw new InvalidOperationException(
                        $"Permission conflict detected for {AssigneeType}:{AssigneeId} " +
                        $"on resource {ResourceId} with action {Action}");
                }

                // ایجاد Permission جدید
                var permission = new Permission(
                     ResourceId,
                     AssigneeType,
                     AssigneeId,
                     Action,
                     effect,
                     EffectiveFrom,
                     ExpiresAt,
                     Description,
                    createdBy: _currentUserContext.UserName ?? "system"
                );

                // ذخیره در Repository
                await _permissionRepository.AddAsync(permission);

                // ذخیره تغییرات
                await AddScopesToPermission(permission.Id, scopes);
                if (rules != null)
                {
                    await _permissionRuleRepository.AddRangeAsync(rules.Select(r => new PermissionRule(permission.Id, r.FieldName, r.Operator, r.Value, r.LogicalOperator, r.GroupOrder, r.JoinLocalKey, r.JoinForeignKey, r.JoinEntity)));
                }

                // انتشار ایونت
                permission.AddDomainEvent(new PermissionChangedEvent(AssigneeId, ResourceId));
                await _unitOfWork.SaveChangesAsync();
                // پاک کردن کش‌های مرتبط
                await InvalidatePermissionCachesAsync(AssigneeId, ResourceId);
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation(
                    "Permission assigned successfully: {PermissionId} for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    permission.Id, AssigneeType, AssigneeId, ResourceId);

                return permission.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign permission for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                     AssigneeType, AssigneeId, ResourceId);
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
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation("Permission revoked successfully: {PermissionId}", permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke permission: {PermissionId}", permissionId);
                throw;
            }
        }

        public async Task DeletePermissionAsync(Guid permissionId)
        {
            try
            {
                _logger.LogInformation("Starting permission revocation: {PermissionId}", permissionId);
                
                await _permissionRepository.DeleteAsync(permissionId);

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation("Permission revoked successfully: {PermissionId}", permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke permission: {PermissionId}", permissionId);
                throw;
            }
        }

        private async Task InvalidatePermissionCachesAsync()
        {
            try
            {
                await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");

                _logger.LogDebug("Invalidated permission caches ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating permission caches  ");
            }
        }

        public async Task UpdatePermissionAsync(Guid Id,
        Guid? ResourceId = null,
        AssigneeType? AssigneeType = null,
        Guid? AssigneeId = null,
        PermissionAction? Action = null,
        PermissionEffect? effect = null,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        bool? IsActive = null,
        string? Description = null,
        List<ScopeType>? scopes = null, List<PermissionRuleCreateDto>? rules = null

        )
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission toggle");

                var permission = await _permissionRepository.GetByIdAsync(Id);
                if (permission == null)
                {
                    throw new ArgumentException($"Permission with ID {Id} not found");
                }
                permission.ApplyChange(ResourceId, AssigneeType, AssigneeId, Action, effect, EffectiveFrom, ExpiresAt, IsActive, Description);
                // انتشار ایونت
                permission.AddDomainEvent(new PermissionChangedEvent((Guid)AssigneeId, (Guid)ResourceId));
                await _permissionRepository.UpdateAsync(permission);
                await UpdateScopeOfPermission(permission.Id, scopes);
                await UpdateRuleOfPermission(permission.Id, rules);
                await _unitOfWork.SaveChangesAsync();
                await InvalidatePermissionCachesAsync();

                _logger.LogInformation("Permission toggled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to toggle permission");
                throw;
            }
        }

        public async Task<PermissionDto> GetById(Guid permissionId)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId, p => p.Scopes , p=>p.Rules);

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

        public async Task<bool> CheckPermissionConflictAsync(
             Guid ResourceId,
             Guid AssigneeId,
             AssigneeType AssigneeType,
             PermissionAction Action
            )
        {
            try
            {
                var conflictSpec = new PermissionConflictSpec(
                     AssigneeType,
                     AssigneeId,
                     ResourceId,
                     Action);

                var existingPermissions = await _permissionSpecRepository.ListBySpecAsync(conflictSpec);
                var hasConflict = existingPermissions.Any();

                _logger.LogDebug(
                    "Permission conflict check for {AssigneeType}:{AssigneeId} on {ResourceId}:{Action} = {HasConflict}",
                     AssigneeType, AssigneeId, ResourceId, Action, hasConflict);

                return hasConflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking permission conflict for {AssigneeType}:{AssigneeId} on {ResourceId}",
                     AssigneeType, AssigneeId, ResourceId);
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
                await InvalidatePermissionCachesAsync();

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

            return new PermissionDto()
            {
                Id = permission.Id,
                ResourceId = permission.ResourceId,
                ResourceKey = permission.Resource?.Key,
                AssigneeType = permission.AssigneeType,
                AssigneeId = permission.AssigneeId,
                Action = permission.Action,
                Effect = permission.Effect,
                IsActive = permission.IsActive,
                EffectiveFrom = permission.EffectiveFrom,
                ExpiresAt = permission.ExpiresAt,
                Description = permission.Description,
                Scopes = permission.Scopes?.Select(p => new ScopeDto
                {
                    scope = p.scope,
                    PermissionId = permission.Id
                }).ToList(),
                rules = permission.Rules?.Select(r => new PermissionRuleDto
                {
                    PermissionId = permission.Id,
                    FieldName = r.FieldName,
                    JoinEntity=r.JoinEntity,
                    JoinForeignKey=r.JoinForeignKey,
                    GroupOrder = r.GroupOrder,
                    Id = r.Id,
                    JoinLocalKey=r.JoinLocalKey,
                    LogicalOperator = r.LogicalOperator,
                    Operator = r.Operator,
                    Value = r.Value
                    
                }).ToList(),
            };
        }

        public async Task SeedRolePermissionsAsync(List<PermissionDto> permissions, CancellationToken cancellationToken = default)
        {
            //var initializeruser = await _userService.GetUserId("intitializer");
            foreach (var permissionDefinition in permissions)
            {
                ResourceByKeySpec specByKey = new ResourceByKeySpec(permissionDefinition.ResourceKey);
                var specRet = await _resourceSpecRepository.FindBySpecAsync(specByKey);
                Permission permission = new Permission(
                    specRet.Items.FirstOrDefault().Id, permissionDefinition.AssigneeType,
                    permissionDefinition.AssigneeId,
                    permissionDefinition.Action,
                    //permissionDefinition.Scope.ToEnumOrDefault(ScopeType.Self),
                    //null,
                    permissionDefinition.Effect
                    );

                //permission.SetUserOwner(initializeruser);
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
                    //p.Scope == permission.Scope &&
                    //p.SpecificScopeId == permission.SpecificScopeId && // همیشه مقدار دارد
                    p.Action == permission.Action &&
                    p.AssigneeType == permission.AssigneeType &&
                    p.AssigneeId == permission.AssigneeId &&
                    p.Effect == permission.Effect
         );
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(List<Guid>? roleIds)
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

        public async Task<IReadOnlyList<PermissionDto>> GetUserAllPermissionsAsync(Guid userId, Guid? personId, List<Guid>? positionsId, List<Guid> roleIds)
        {
            try
            {
                var spec = new UserPermissionsSpec(userId, personId, positionsId, roleIds);
                var allPermissions = await _permissionSpecRepository.ListBySpecAsync(spec);


                var dtos = allPermissions.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} permissions for Position {userId}",
                    dtos.Count, userId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for position {userId}", userId);
                throw;
            }
        }

        public async Task<IReadOnlyList<PermissionDto>> GetPermissions(GetPermissionsQuery request)
        {

            var cacheKey = $"{baseCacheKey}:full";
            var cached = await _cache.GetAsync<IReadOnlyList<PermissionDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for full resource tree");
                return cached;
            }
            var spec = new GetPermissionsSpec(request.AssigneeType, request.AssigneeId, request.ResourceId, request.description);
            var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);


            var result = permissions.Select(MapToDto).ToList();


            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;

        }
    }
}