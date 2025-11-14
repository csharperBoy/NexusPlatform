using Authorization.Application.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IPermissionService
    {
        Task RegisterPermissionsAsync(IEnumerable<PermissionDescriptor> descriptors, CancellationToken ct = default);
        Task<IEnumerable<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
        Task InvalidateUserCacheAsync(Guid userId);
        Task InvalidateRoleCacheAsync(Guid roleId);
    }
}
