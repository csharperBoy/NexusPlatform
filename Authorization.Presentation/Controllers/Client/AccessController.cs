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
//using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;
namespace Authorization.Presentation.Controllers.Client
{
    [ApiController]
    [Route("api/access")]
    [Authorize]
    public class AccessController : BaseController
    {
        

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
