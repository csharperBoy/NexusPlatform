using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Interfaces
{
    public interface IAuthorizationService
    {
        // مدیریت نقش‌ها
        Task<Result> AssignRoleToUserAsync(Guid userId, string roleName);
        Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName);
        Task<Result> AssignDefaultRoleAsync(Guid userId);
        Task<bool> UserHasRoleAsync(Guid userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(Guid userId);

        // مدیریت permissions (برای آینده)
        Task<bool> UserHasPermissionAsync(Guid userId, string permission);
        Task<Result> AssignPermissionToUserAsync(Guid userId, string permission);
        Task<Guid> GetRoleId(string roleName);

    }
}
