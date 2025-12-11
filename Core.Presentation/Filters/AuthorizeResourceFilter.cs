using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
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
        private readonly IAuthorizationChecker _authorizationChecker;
        private readonly ICurrentUserService _currentUserService;
        private readonly string _resourceKey;
        private readonly string _action;

        // Constructor برای استفاده توسط Factory
        public AuthorizeResourceFilter(
            IAuthorizationChecker authorizationChecker,
            ICurrentUserService currentUserService,
            string resourceKey,
            string action)
        {
            _authorizationChecker = authorizationChecker;
            _currentUserService = currentUserService;
            _resourceKey = resourceKey;
            _action = action;
        }

        // Constructor دوم برای استفاده توسط DI (بدون پارامترهای string)
       /* public AuthorizeResourceFilter(
            IAuthorizationChecker authorizationChecker,
            ICurrentUserService currentUserService)
        {
            _authorizationChecker = authorizationChecker;
            _currentUserService = currentUserService;
            _resourceKey = string.Empty;
            _action = string.Empty;
        }*/

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // اگر resourceKey و action خالی باشند، از metadata endpoint بگیر
         /*   if (string.IsNullOrEmpty(_resourceKey))
            {
                var endpoint = context.HttpContext.GetEndpoint();
                var attribute = endpoint?.Metadata.GetMetadata<AuthorizeResourceAttribute>();

                if (attribute != null)
                {
                    //_resourceKey = attribute.ResourceKey;
                    //_action = attribute.Action;
                }
            }
         */
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
