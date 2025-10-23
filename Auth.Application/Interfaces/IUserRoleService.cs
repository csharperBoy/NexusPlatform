using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<Result> AssignRoleToUserAsync(Guid userId, string roleName);
        Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName);
        Task<Result> AssignDefaultRoleAsync(Guid userId);
        Task<bool> UserHasRoleAsync(Guid userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(Guid userId);
    }
}
