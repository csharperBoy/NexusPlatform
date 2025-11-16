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
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId:guid}/permissions/effective")]
        public async Task<IActionResult> GetEffectivePermissions(Guid userId)
        {
            var res = await _mediator.Send(new GetEffectivePermissionsQuery(userId));

            if (!res.Succeeded)
                return BadRequest(res.Error);

            return Ok(res.Data);
        }

        [HttpPost("{userId:guid}/permissions")]
        public async Task<IActionResult> AddUserPermission(Guid userId, AddUserPermissionOverrideCommand cmd)
        {
            cmd.UserId = userId;

            var res = await _mediator.Send(cmd);

            if (!res.Succeeded)
                return BadRequest(res.Error);

            return Ok(res.Data);
        }

        [HttpDelete("{userId:guid}/permissions/{overrideId:guid}")]
        public async Task<IActionResult> DeleteUserOverride(Guid userId, Guid overrideId)
        {
            var res = await _mediator.Send(new DeleteUserPermissionOverrideCommand(userId, overrideId));

            if (!res.Succeeded)
                return BadRequest(res.Error);

            return Ok(res.Data);
        }
    }
}
