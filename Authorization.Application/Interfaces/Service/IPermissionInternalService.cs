using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Queries.Permissions;
using Authorization.Domain.Entities;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces.Service
{
    public interface IPermissionInternalService : IPermissionPublicService
    {
        // عملیات Write با منطق پیچیده
        Task<Guid> AssignPermissionAsync(
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
            );
        Task RevokePermissionAsync(Guid permissionId);
        Task DeletePermissionAsync(Guid permissionId);
        Task UpdatePermissionAsync(Guid Id,
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
       );

        // عملیات Read
        Task<PermissionDto> GetById(Guid permissionId);
        Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId);
        Task<IReadOnlyList<PermissionDto>> GetPermissions(GetPermissionsQuery request);

        // منطق کسب‌وکار پیچیده
        Task<bool> CheckPermissionConflictAsync(
             Guid ResourceId,
        Guid AssigneeId,
        AssigneeType AssigneeType,
        PermissionAction Action 

            );
        Task ResolvePermissionConflictsAsync(Guid userId, string resourceKey);
        Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(List<Guid>? roleIds);
        Task<IReadOnlyList<PermissionDto>> GetPersonPermissionsAsync(Guid? personId);
        Task<IReadOnlyList<PermissionDto>> GetPositionPermissionsAsync(List<Guid>? positionIds);
        //Task<Permission?> GetById(Guid id);
    }
}
