using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Permissions;
using Core.Application.Abstractions.Authorization;
using Core.Shared.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IPermissionInternalService : IPermissionPublicService
    {
        // عملیات Write با منطق پیچیده
        Task<Guid> AssignPermissionAsync(AssignPermissionCommand command);
        Task RevokePermissionAsync(Guid permissionId);
        Task TogglePermissionAsync(Guid permissionId, bool isActive);

        // عملیات Read
        Task<PermissionDto> GetPermissionAsync(Guid permissionId);
        Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId);

        // منطق کسب‌وکار پیچیده
        Task<bool> CheckPermissionConflictAsync(AssignPermissionCommand command);
        Task ResolvePermissionConflictsAsync(Guid userId, string resourceKey);
        Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(List<Guid>? roleIds);
        Task<IReadOnlyList<PermissionDto>> GetPersonPermissionsAsync(Guid? personId);
        Task<IReadOnlyList<PermissionDto>> GetPositionPermissionsAsync(List<Guid>? positionIds);
    }
}
