using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Presentation.Filters
{
    public class AuthorizeResourceFilter : IAsyncAuthorizationFilter
    {
        private readonly string _resourceKey;
        private readonly string _action;
        private readonly IAuthorizationChecker _authorizationChecker; // ✅ تغییر به IAuthorizationChecker
        private readonly ICurrentUserService _currentUserService;

        public AuthorizeResourceFilter(
            string resourceKey,
            string action,
            IAuthorizationChecker authorizationChecker, // ✅ تغییر
            ICurrentUserService currentUserService)
        {
            _resourceKey = resourceKey;
            _action = action;
            _authorizationChecker = authorizationChecker;
            _currentUserService = currentUserService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = _currentUserService.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasAccess = await _authorizationChecker.CheckAccessAsync(userId.Value, _resourceKey, _action);
            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
