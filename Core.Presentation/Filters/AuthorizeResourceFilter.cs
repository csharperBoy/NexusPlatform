using Core.Application.Abstractions.Authorization.Processor;
using Core.Application.Context;
using Core.Application.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Core.Presentation.Filters
{
    public class AuthorizeResourceFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationProcessor _authorizationChecker;
        private readonly UserDataContext _userDataContext;
        private readonly string _resourceKey;
        private readonly string _action;
        private readonly IHttpContextAccessor _httpContext;


        private readonly IUserDataContextProvider _provider;

        public AuthorizeResourceFilter(
            IAuthorizationProcessor authorizationChecker, IHttpContextAccessor httpContext, IUserDataContextProvider provider,
        UserDataContext userDataContext,
        string resourceKey,
            string action)
        {
            _authorizationChecker = authorizationChecker;
            _userDataContext = userDataContext;
            _resourceKey = resourceKey;
            _action = action;
            _httpContext = httpContext;
            _provider = provider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await SetUserData();
            var userId = _userDataContext.UserId;
             //var userIdstr = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //Guid userId = string.IsNullOrEmpty(userIdstr) ? Guid.Empty : Guid.Parse(userIdstr);

            if (userId == null || userId == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasAccess = await _authorizationChecker.CheckAccessAsync( _resourceKey, _action );
            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
        private async Task SetUserData()
        {

            CancellationToken cancellationToken = new CancellationToken();
             await _provider.SetUserData(cancellationToken);

            
        }
    }
}
