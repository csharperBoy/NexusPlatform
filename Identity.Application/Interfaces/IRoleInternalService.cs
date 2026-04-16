using Core.Application.Abstractions.Identity.PublicService;
using Core.Shared.DTOs.Identity;
using Core.Shared.Results;
using Identity.Application.Commands.Role;
using Identity.Application.Commands.User;
using Identity.Application.DTOs;
using Identity.Application.Queries.Role;
using Identity.Application.Queries.User;
using Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Interfaces
{
    public interface IRoleInternalService:IRolePublicService
    {
        Task<Result> AssignRoleToUserAsync(Guid userId, string roleName);
        Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName);
        Task<Result> AssignDefaultRoleAsync(Guid userId);
        Task<bool> UserHasRoleAsync(Guid userId, string roleName);
        Task<IList<RoleDto>> GetUserRolesAsync(Guid userId);
        Task<Guid> GetRoleId(string roleName);



        Task<Guid> CreateRoleAsync(CreateRoleCommand request);
        Task DeleteRoleAsync(Guid id);

        Task<ApplicationRole?> GetById(Guid id);
        Task<IReadOnlyList<RoleDto>> getRoles(GetRolesQuery request);
        Task UpdateRoleAsync(UpdateRoleCommand request);
    }
}
