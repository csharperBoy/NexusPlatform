using Authorization.Application.Commands.Permissions;
using Authorization.Application.DTOs.Users;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Permissions;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
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

namespace Authorization.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/authorization/admin/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class PermissionsController : BaseController
    {
        /// <summary>
        /// 👤 دریافت دسترسی‌های یک کاربر
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> GetPermissionsByUser(Guid userId)
        {
            var query = new GetPermissionsByUserQuery(userId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 📄 دریافت دسترسی‌های یک منبع
        /// </summary>
        [HttpGet("resource/{resourceId:guid}")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> GetPermissionsByResource(Guid resourceId)
        {
            var query = new GetPermissionsByResourceQuery(resourceId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// ➕ اختصاص دسترسی جدید
        /// </summary>
        [HttpPost("assign")]
        [AuthorizeResource("authorization.permissions", "Create")]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// ➖ لغو دسترسی
        /// </summary>
        [HttpPost("revoke")]
        [AuthorizeResource("authorization.permissions", "Delete")]
        public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// 🔄 تغییر وضعیت دسترسی
        /// </summary>
        [HttpPatch("toggle")]
        [AuthorizeResource("authorization.permissions", "Edit")]
        public async Task<IActionResult> TogglePermission([FromBody] TogglePermissionCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// ✅ بررسی دسترسی
        /// </summary>
        [HttpPost("check")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 📊 خلاصه دسترسی‌های کاربر
        /// </summary>
        [HttpGet("user/{userId:guid}/summary")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> GetUserPermissionsSummary(Guid userId)
        {
            // استفاده از AuthorizationService برای دریافت خلاصه دسترسی‌ها
            var authorizationService = HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var userAccess = await authorizationService.GetUserEffectiveAccessAsync(userId);

            var summary = new UserPermissionsSummaryDto
            {
                UserId = userId,
                AllowedResources = userAccess.Permissions
                    .Where(p => p.CanView)
                    .Select(p => p.ResourceKey)
                    .ToList(),
                DeniedResources = userAccess.Permissions
                    .Where(p => !p.CanView)
                    .Select(p => p.ResourceKey)
                    .ToList(),
                HasFullSystemAccess = userAccess.Permissions.Any(p =>
                    p.ResourceKey == "system" && p.CanView && p.CanCreate && p.CanEdit && p.CanDelete)
            };

            return Ok(summary);
        }

        // ========== APIهای نیازمند توسعه ==========

        /*
        /// <summary>
        /// 🏢 دریافت دسترسی‌های یک نقش
        /// </summary>
        [HttpGet("role/{roleId:guid}")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> GetPermissionsByRole(Guid roleId)
        {
            // ⚠️ نیاز به کوئری جدید: GetPermissionsByRoleQuery
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// ✏️ به‌روزرسانی دسترسی
        /// </summary>
        [HttpPut("update")]
        [AuthorizeResource("authorization.permissions", "Edit")]
        public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionCommand command)
        {
            // ⚠️ نیاز به کامند جدید: UpdatePermissionCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🗑️ حذف دسترسی
        /// </summary>
        [HttpDelete("{permissionId:guid}")]
        [AuthorizeResource("authorization.permissions", "Delete")]
        public async Task<IActionResult> DeletePermission(Guid permissionId)
        {
            // ⚠️ نیاز به کامند جدید: DeletePermissionCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// ⏰ تنظیم محدوده زمانی دسترسی
        /// </summary>
        [HttpPatch("set-temporal")]
        [AuthorizeResource("authorization.permissions", "Edit")]
        public async Task<IActionResult> SetTemporalPermission([FromBody] SetTemporalPermissionRequest request)
        {
            // ⚠️ نیاز به کامند جدید: SetTemporalPermissionCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🔍 جستجوی دسترسی‌ها
        /// </summary>
        [HttpGet("search")]
        [AuthorizeResource("authorization.permissions", "View")]
        public async Task<IActionResult> SearchPermissions(
            [FromQuery] string? searchTerm,
            [FromQuery] Guid? resourceId,
            [FromQuery] Guid? assigneeId,
            [FromQuery] AssigneeType? assigneeType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // ⚠️ نیاز به کوئری جدید: SearchPermissionsQuery
            return BadRequest("این API در حال توسعه است");
        }
        */
    }

    // ========== DTOهای درخواست برای APIهای آینده ==========

    /*
    public class SetTemporalPermissionRequest
    {
        public Guid PermissionId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    */
}
