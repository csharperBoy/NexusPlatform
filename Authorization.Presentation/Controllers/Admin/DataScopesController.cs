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
//using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;

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

       
    }

   
}
