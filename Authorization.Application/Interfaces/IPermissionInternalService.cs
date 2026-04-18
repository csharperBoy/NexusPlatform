using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Queries.Permissions;
using Authorization.Domain.Entities;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Shared.DTOs.Authorization;
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
        Task<Guid> AssignPermissionAsync(CreatePermissionCommand command);
        Task RevokePermissionAsync(Guid permissionId);
        Task DeletePermissionAsync(Guid permissionId);
        Task UpdatePermissionAsync(UpdatePermissionCommand request);

        // عملیات Read
        Task<PermissionDto> GetById(Guid permissionId);
        Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId);
        Task<IReadOnlyList<PermissionDto>> GetPermissions(GetPermissionsQuery request);

        // منطق کسب‌وکار پیچیده
        Task<bool> CheckPermissionConflictAsync(CreatePermissionCommand command);
        Task ResolvePermissionConflictsAsync(Guid userId, string resourceKey);
        Task<IReadOnlyList<PermissionDto>> GetRolePermissionsAsync(List<Guid>? roleIds);
        Task<IReadOnlyList<PermissionDto>> GetPersonPermissionsAsync(Guid? personId);
        Task<IReadOnlyList<PermissionDto>> GetPositionPermissionsAsync(List<Guid>? positionIds);
        //Task<Permission?> GetById(Guid id);
    }
}
