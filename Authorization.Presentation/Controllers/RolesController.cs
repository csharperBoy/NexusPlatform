using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Presentation.Controllers
{
    [ApiController]
    [Route("api/authorization/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{roleId:guid}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            var res = await _mediator.Send(new GetRolePermissionsQuery(roleId));

            if (!res.Succeeded)
                return BadRequest(res.Error);

            return Ok(res.Data);
        }

        [HttpPost("{roleId:guid}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(Guid roleId, UpdateRolePermissionsCommand cmd)
        {
            cmd.RoleId = roleId;

            var res = await _mediator.Send(cmd);

            if (!res.Succeeded)
                return BadRequest(res.Error);

            return Ok(res.Data);
        }
    }
}
