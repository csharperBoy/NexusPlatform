using Authorization.Application.Commands.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.DataScopes;
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
    //[Authorize(Policy = "RequireAdminRole")]
    public class DataScopesController : BaseController
    {
        /// <summary>
        /// 👤 دریافت محدوده‌های داده کاربر
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        //[AuthorizeResource("authorization.datascopes", "View")]
        [AuthorizeResource("authorization", "View")]
        public async Task<IActionResult> GetDataScopesByUser(Guid userId)
        {
            var query = new GetDataScopesByUserQuery(userId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 📄 دریافت محدوده‌های داده یک منبع
        /// </summary>
        [HttpGet("resource/{resourceId:guid}")]
        [AuthorizeResource("authorization.datascopes", "View")]
        public async Task<IActionResult> GetDataScopeByResource(Guid resourceId)
        {
            var query = new GetDataScopeByResourceQuery(resourceId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// ➕ اختصاص محدوده داده جدید
        /// </summary>
        [HttpPost("assign")]
        [AuthorizeResource("authorization.datascopes", "Create")]
        public async Task<IActionResult> AssignDataScope([FromBody] AssignDataScopeCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// ✏️ به‌روزرسانی محدوده داده
        /// </summary>
        [HttpPut("update")]
        [AuthorizeResource("authorization.datascopes", "Edit")]
        public async Task<IActionResult> UpdateDataScope([FromBody] UpdateDataScopeCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// 🔍 ارزیابی محدوده داده
        /// </summary>
        [HttpPost("evaluate")]
        [AuthorizeResource("authorization.datascopes", "View")]
        public async Task<IActionResult> EvaluateDataScope([FromBody] EvaluateDataScopeRequest request)
        {
            // استفاده از DataScopeEvaluator
            var dataScopeEvaluator = HttpContext.RequestServices
                .GetRequiredService<IDataScopeEvaluator>();

            var dataScope = await dataScopeEvaluator.EvaluateDataScopeAsync(
                request.UserId,
                request.ResourceKey);

            return Ok(dataScope);
        }

        /// <summary>
        /// 📊 دریافت محدوده‌های مؤثر کاربر
        /// </summary>
        [HttpGet("user/{userId:guid}/effective")]
        [AuthorizeResource("authorization.datascopes", "View")]
        public async Task<IActionResult> GetUserEffectiveDataScopes(Guid userId)
        {
            var dataScopeEvaluator = HttpContext.RequestServices
                .GetRequiredService<IDataScopeEvaluator>();

            var dataScopes = await dataScopeEvaluator.EvaluateAllDataScopesAsync(userId);
            return Ok(dataScopes);
        }

        // ========== APIهای نیازمند توسعه ==========

        /*
        /// <summary>
        /// 🏢 دریافت محدوده‌های داده یک نقش
        /// </summary>
        [HttpGet("role/{roleId:guid}")]
        [AuthorizeResource("authorization.datascopes", "View")]
        public async Task<IActionResult> GetDataScopesByRole(Guid roleId)
        {
            // ⚠️ نیاز به کوئری جدید: GetDataScopesByRoleQuery
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🗑️ حذف محدوده داده
        /// </summary>
        [HttpDelete("{dataScopeId:guid}")]
        [AuthorizeResource("authorization.datascopes", "Delete")]
        public async Task<IActionResult> DeleteDataScope(Guid dataScopeId)
        {
            // ⚠️ نیاز به کامند جدید: DeleteDataScopeCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// ⏰ تنظیم محدوده زمانی
        /// </summary>
        [HttpPatch("set-temporal")]
        [AuthorizeResource("authorization.datascopes", "Edit")]
        public async Task<IActionResult> SetTemporalDataScope([FromBody] SetTemporalDataScopeRequest request)
        {
            // ⚠️ نیاز به کامند جدید: SetTemporalDataScopeCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🔄 فعال/غیرفعال کردن محدوده داده
        /// </summary>
        [HttpPatch("toggle-active")]
        [AuthorizeResource("authorization.datascopes", "Edit")]
        public async Task<IActionResult> ToggleDataScopeActive([FromBody] ToggleDataScopeActiveRequest request)
        {
            // ⚠️ نیاز به کامند جدید: ToggleDataScopeActiveCommand
            return BadRequest("این API در حال توسعه است");
        }
        */
    }

    // ========== DTOهای درخواست ==========

    public class EvaluateDataScopeRequest
    {
        public Guid UserId { get; set; }
        public string ResourceKey { get; set; } = string.Empty;
    }

    /*
    public class SetTemporalDataScopeRequest
    {
        public Guid DataScopeId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ToggleDataScopeActiveRequest
    {
        public Guid DataScopeId { get; set; }
        public bool IsActive { get; set; }
    }
    */
}
