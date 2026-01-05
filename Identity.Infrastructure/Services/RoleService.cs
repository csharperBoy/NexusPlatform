using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Identity.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    public class RoleService : IRoleInternalService, IRolePublicService
    {
        private readonly IAuthorizationService _authorizationService;

        public RoleService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }


        public async Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default)
        {
            return await _authorizationService.GetRoleId("Admin");
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            return await _authorizationService.GetUserRolesAsync(userId);
        }
    }

}