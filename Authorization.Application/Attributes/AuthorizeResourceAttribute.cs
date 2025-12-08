using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Application.Attributes
{
    /// <summary>
    /// اعمال کنترل دسترسی بر اساس Resource و Action
    /// از ServiceFilterAttribute استفاده می‌کند تا بتواند Dependency Injection داشته باشد
    /// </summary>
    public class AuthorizeResourceAttribute : ServiceFilterAttribute  
    {
        public AuthorizeResourceAttribute(string resourceKey, string action = "View")
            : base(typeof(AuthorizeResourceFilter))
        {
            ResourceKey = resourceKey;
            Action = action;
        }

        public string ResourceKey { get; }
        public string Action { get; }
    }

    /// <summary>
    /// فیلتر اصلی برای بررسی دسترسی
    /// از IAsyncAuthorizationFilter استفاده می‌کند برای پشتیبانی از async/await
    /// </summary>
    public class AuthorizeResourceFilter : IAsyncAuthorizationFilter  // 📍 از Microsoft.AspNetCore.Mvc.Filters
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        // Constructor Injection - بهترین روش برای وابستگی‌ها
        public AuthorizeResourceFilter(
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService)
        {
            _authorizationService = authorizationService;
            _currentUserService = currentUserService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)  // 📍 از Microsoft.AspNetCore.Mvc.Filters
        {
            // پیدا کردن attribute مربوطه
            var attribute = context.Filters
                .OfType<AuthorizeResourceAttribute>()
                .FirstOrDefault(f => f is AuthorizeResourceAttribute);

            if (attribute == null)
            {
                context.Result = new UnauthorizedResult();  // 📍 از Microsoft.AspNetCore.Mvc
                return;
            }

            var resourceKey = attribute.ResourceKey;
            var action = attribute.Action;

            // بررسی کاربر جاری
            var userId = _currentUserService.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // بررسی دسترسی به صورت async
            var hasAccess = await _authorizationService.CheckAccessAsync(userId.Value, resourceKey, action);
            if (!hasAccess)
            {
                context.Result = new ForbidResult();  // 📍 از Microsoft.AspNetCore.Mvc
            }
        }
    }
}
