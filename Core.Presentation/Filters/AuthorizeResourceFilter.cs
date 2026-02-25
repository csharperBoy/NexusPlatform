using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Security;
using Core.Application.Provider;
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
        private readonly IAuthorizationProcessor _authorizationChecker;
        private readonly IDataScopeContextProvider _contextProvider;
        private readonly string _resourceKey;
        private readonly string _action;

        // Constructor برای استفاده توسط Factory
        public AuthorizeResourceFilter(
            IAuthorizationProcessor authorizationChecker,
            IDataScopeContextProvider contextProvider,
        string resourceKey,
            string action)
        {
            _authorizationChecker = authorizationChecker;
            _contextProvider = contextProvider;
            _resourceKey = resourceKey;
            _action = action;
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
