using Authorization.Application.Commands.PermissionRule;
using Authorization.Application.Commands.Permissions;
using Authorization.Application.Queries.PermissionRule;
using Authorization.Application.Queries.Permissions;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Presentation.Controllers.Admin
{
   /*
    [ApiController]
    [Route("api/authorization/admin/[controller]")]
    //[Authorize(Policy = "RequireAdminRole")]
    public class PermissionRuleController : BaseController
    {
        #region crud api
        [HttpGet("getRules")]
        [AuthorizeResource("authorization.permission", "View")]
        public async Task<IActionResult> getRules([FromQuery] GetPermissionRulesQuery? request = null)
        {

            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("{id:guid}")]
        [AuthorizeResource("authorization.permission", "View")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetPermissionRuleByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }


        /// <summary>
        /// ✏️ به‌روزرسانی نقش
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizeResource("authorization.permission", "Edit")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionRuleCommand command)
        {
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPost("Create")]
        [AuthorizeResource("authorization.permission", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePermissionRuleCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        /// <summary>
        /// 🗑️ حذف نقش
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("authorization.permission", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePermissionRuleCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        #endregion
        
    }
   */
}
