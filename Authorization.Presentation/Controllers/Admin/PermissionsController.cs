using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/authorization/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var result = await _mediator.Send(new GetAllPermissionsQuery());

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }
    }
}
