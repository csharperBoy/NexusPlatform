using Authorization.Application.Commands;
using Authorization.Application.Commands.Resource;
using Authorization.Application.DTOs.Resource;
using Authorization.Application.Queries.Resource;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.DTOs.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

//using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;


namespace Authorization.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/authorization/admin/[controller]")]
    //[Authorize(Policy = "RequireAdminRole")]
    public class ResourcesController : BaseController
    {
        /// <summary>
        /// 📋 دریافت درخت کامل منابع
        /// </summary>
        [HttpGet("tree")]
        [AuthorizeResource("authorization.resource", "View")]
        public async Task<IActionResult> GetResourceTree([FromQuery] Guid? rootId = null)
        {
          
            var query = new GetResourceTreeQuery(rootId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
        /// <summary>
        /// 🆕 ایجاد منبع جدید
        /// </summary>
        [HttpPost("Create")]
        [AuthorizeResource("authorization.resource", "Create")]
        public async Task<IActionResult> CreateResource([FromBody] CreateResourceCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        /// <summary>
        /// 🔍 دریافت منبع براساس ID 
        /// </summary>
        [HttpGet("{id:guid}")]
        [AuthorizeResource("authorization.resource", "View")]
        public async Task<IActionResult> GetResourceById(Guid id)
        {
            var query = new GetResourceByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("GetSelectionList")]
        //[AuthorizeResource("authorization.resource", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetResourcesSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }

        /// <summary>
        /// ✏️ به‌روزرسانی منبع
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizeResource("authorization.resource", "Edit")]
        public async Task<IActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        /// <summary>
        /// 🗑️ حذف منبع
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("authorization.resource", "Delete")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var command = new DeleteResourceCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

    }

   
}
