using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Identity.Application.Commands;
using Identity.Application.Commands.Role;
using Identity.Application.Queries;
using Identity.Application.Queries.Role;
using Identity.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/[controller]")]
    public class RolesController : BaseController
    {
       
        [HttpGet("GetRoles")]
        [AuthorizeResource("identity.role", "View")]
        public async Task<IActionResult> GetRoles([FromQuery] GetRolesQuery? request = null)
        {

            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("{id:guid}")]
        [AuthorizeResource("identity.role", "View")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var query = new GetRoleByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }


        /// <summary>
        /// ✏️ به‌روزرسانی نقش
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizeResource("identity.role", "Edit")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
        {
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPost("Create")]
        [AuthorizeResource("identity.role", "Create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        /// <summary>
        /// 🗑️ حذف نقش
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("identity.role", "Delete")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var command = new DeleteRoleCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign([FromBody] AssignRoleCommand command)
        {
            var res = await Mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok();
        }

    }
}
