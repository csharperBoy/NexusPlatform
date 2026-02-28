using Authorization.Application.Queries.Permissions;
using Core.Application.Abstractions.Security;
using Core.Application.Context;
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
    [Route("api/authorization")]
    [Authorize]
    public class AuthorizationController : BaseController
    {
        /// <summary>
        /// ✅ بررسی دسترسی
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionQuery query)
        {
            // اگر UserId مشخص نشده، از کاربر جاری استفاده کن
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<UserDataContext>();

            if (query.UserId == Guid.Empty && currentUserService.UserId != Guid.Empty)
            {
                query = query with { UserId = currentUserService.UserId };
            }

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 📋 دریافت دسترسی‌های من
        /// </summary>
        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var currentUserService = HttpContext.RequestServices
                .GetRequiredService<UserDataContext>();

            if (currentUserService.UserId == Guid.Empty)
                return Unauthorized();

            var query = new GetPermissionsByUserQuery(currentUserService.UserId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

      


        // ========== APIهای نیازمند توسعه ==========

        /*
        /// <summary>
        /// 🔄 بررسی دسترسی گروهی
        /// </summary>
        [HttpPost("check-batch")]
        public async Task<IActionResult> CheckBatch([FromBody] CheckBatchRequest request)
        {
            // ⚠️ نیاز به کوئری جدید: CheckBatchPermissionsQuery
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🏷️ دریافت نقش‌های من
        /// </summary>
        [HttpGet("my-roles")]
        public async Task<IActionResult> GetMyRoles()
        {
            // ⚠️ خارج از scope فعلی - ماژول Identity
            return BadRequest("این API در ماژول Identity پیاده‌سازی می‌شود");
        }
        */
    }

    // ========== DTOهای درخواست ==========

    public class CheckByRouteRequest
    {
        public string ResourceKey { get; set; } = string.Empty;
        public string Action { get; set; } = "View";
    }

    /*
    public class CheckBatchRequest
    {
        public List<PermissionCheck> Checks { get; set; } = new();
    }

    public class PermissionCheck
    {
        public string ResourceKey { get; set; } = string.Empty;
        public string Action { get; set; } = "View";
    }
    */
}
