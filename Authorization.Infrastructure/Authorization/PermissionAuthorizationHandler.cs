using Authorization.Application.Interfaces;
using Authorization.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Authorization
{
    public class PermissionAuthorizationHandler
         : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissions;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(IPermissionService permissions,
                                             ILogger<PermissionAuthorizationHandler> logger)
        {
            _permissions = permissions;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return;
            }

            var has = await _permissions.UserHasPermissionAsync(userId, requirement.PermissionCode);

            if (has)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}
