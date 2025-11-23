using Authorization.Application.DTOs;
using Authorization.Application.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IAuthorizationService
    {
        Task RegisterPermissionsAsync(IEnumerable<PermissionDescriptor> descriptors, CancellationToken ct = default);
        Task<IEnumerable<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
        Task InvalidateUserCacheAsync(Guid userId);
        Task InvalidateRoleCacheAsync(Guid roleId);

        Task<List<PermissionDto>> GetAllAsync(CancellationToken ct);
        Task<List<Guid>> GetRolePermissionIdsAsync(Guid roleId, CancellationToken ct);
        Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, CancellationToken ct);
        Task AddUserOverrideAsync(Guid userId, Guid permissionId, bool granted, string? scope, CancellationToken ct);
        Task RemoveUserOverrideAsync(Guid userId, Guid overrideId, CancellationToken ct);
    }
}
