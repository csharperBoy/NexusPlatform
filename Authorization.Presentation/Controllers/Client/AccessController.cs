using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Users;
using Authorization.Domain.Entities;
using Core.Application.Abstractions.Security;
using Core.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;
namespace Authorization.Presentation.Controllers.Client
{
    [ApiController]
    [Route("api/access")]
    [Authorize]
    public class AccessController : BaseController
    {
        /// <summary>
        /// 📋 دریافت منابع قابل دسترسی من
        /// </summary>
        [HttpGet("my-resources")]
        public async Task<IActionResult> GetMyResources()
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var authorizationService = HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var userAccess = await authorizationService.GetUserEffectiveAccessAsync(
                currentUserService.UserId.Value);

            return Ok(userAccess.Permissions);
        }

        /// <summary>
        /// 📱 دریافت منابع قابل دسترسی براساس نوع
        /// </summary>
        [HttpGet("my-resources/type/{type}")]
        public async Task<IActionResult> GetMyResourcesByType(string type)
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var authorizationService = HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var userAccess = await authorizationService.GetUserEffectiveAccessAsync(
                currentUserService.UserId.Value);

            var filtered = userAccess.Permissions
                .Where(p => p.ResourceKey.Split('.')
                    .LastOrDefault()?.StartsWith(type, StringComparison.OrdinalIgnoreCase) ?? false)
                .ToList();

            return Ok(filtered);
        }

        /// <summary>
        /// 🗂️ دریافت منابع قابل دسترسی براساس دسته‌بندی
        /// </summary>
        [HttpGet("my-resources/category/{category}")]
        public async Task<IActionResult> GetMyResourcesByCategory(string category)
        {
            // مشابه بالا، فیلتر براساس category
            return BadRequest("این API نیاز به توسعه دارد");
        }

        /// <summary>
        /// 📊 دریافت محدوده‌های داده من
        /// </summary>
        [HttpGet("my-data-scopes")]
        public async Task<IActionResult> GetMyDataScopes()
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var dataScopeEvaluator = HttpContext.RequestServices
                .GetRequiredService<IDataScopeEvaluator>();

            var dataScopes = await dataScopeEvaluator.EvaluateAllDataScopesAsync(
              );

            return Ok(dataScopes);
        }

        /// <summary>
        /// 🔍 دریافت محدوده داده برای یک منبع
        /// </summary>
        [HttpGet("my-data-scopes/resource/{resourceKey}")]
        public async Task<IActionResult> GetMyDataScopeByResource(string resourceKey)
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var query = new GetUserDataScopeByResourceQuery(
                currentUserService.UserId.Value,
                resourceKey);

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// ⚙️ دریافت تنظیمات دسترسی من
        /// </summary>
        [HttpGet("my-access-settings")]
        public async Task<IActionResult> GetMyAccessSettings()
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var userId = currentUserService.UserId.Value;

            // ترکیب چند سرویس
            var authorizationService = HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var dataScopeEvaluator = HttpContext.RequestServices
                .GetRequiredService<IDataScopeEvaluator>();

            // اجرای موازی دو task
            var userAccessTask = authorizationService.GetUserEffectiveAccessAsync(userId);
            var dataScopesTask = dataScopeEvaluator.EvaluateAllDataScopesAsync();

            await Task.WhenAll(userAccessTask, dataScopesTask);

            var userAccess = userAccessTask.Result;
            var dataScopes = dataScopesTask.Result;

            var settings = new
            {
                UserId = userId,
                TotalPermissions = userAccess.Permissions.Count,
                TotalDataScopes = dataScopes.Count,
                HasFullAccess = userAccess.Permissions.Any(p =>
                    p.ResourceKey == "system" && p.CanView && p.CanCreate && p.CanEdit && p.CanDelete),
                LastEvaluated = DateTime.UtcNow
            };

            return Ok(settings);
        }

        /// <summary>
        /// 🛡️ بررسی دسترسی صفحه
        /// </summary>
        [HttpPost("check-page-access")]
        public async Task<IActionResult> CheckPageAccess([FromBody] CheckPageAccessRequest request)
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.UserId.HasValue)
                return Unauthorized();

            var authorizationService = HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var permissions = request.RequiredPermissions
                .Select(p => (p.ResourceKey, p.Action))
                .ToList();

            var hasAccess = await authorizationService.CheckMultipleAccessAsync(
                currentUserService.UserId.Value,
                permissions);

            return Ok(new { HasAccess = hasAccess });
        }

        // ========== APIهای نیازمند توسعه ==========

        /*
        /// <summary>
        /// 🌳 دریافت درخت منابع قابل دسترسی
        /// </summary>
        [HttpGet("my-resources/tree")]
        public async Task<IActionResult> GetMyResourceTree()
        {
            // ⚠️ نیاز به کوئری جدید: GetUserResourceTreeQuery
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🚪 بررسی دسترسی به مسیر
        /// </summary>
        [HttpGet("can-access/{route}")]
        public async Task<IActionResult> CanAccessRoute(string route)
        {
            // ⚠️ نیاز به کوئری جدید: CanAccessRouteQuery
            return BadRequest("این API در حال توسعه است");
        }
        */
    }

    // ========== DTOهای درخواست ==========

    public class CheckPageAccessRequest
    {
        public List<PagePermission> RequiredPermissions { get; set; } = new();
    }

    public class PagePermission
    {
        public string ResourceKey { get; set; } = string.Empty;
        public string Action { get; set; } = "View";
    }
}
