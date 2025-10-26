using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class RoleResolver : IRoleResolver
    {
        private readonly IAuthorizationService _authorizationService;

        public RoleResolver(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            return await _authorizationService.GetUserRolesAsync(userId);
        }
    }

}