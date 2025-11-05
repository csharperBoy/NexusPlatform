using Identity.Application.Commands;
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
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign([FromBody] AssignRoleCommand command)
        {
            var res = await _mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok();
        }

        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles([FromRoute] Guid userId)
        {
            var res = await _mediator.Send(new GetUserRolesQuery(userId));
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }
    }
}
