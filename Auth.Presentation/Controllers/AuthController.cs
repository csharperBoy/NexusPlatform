using Microsoft.AspNetCore.Mvc;
using MediatR;
using Auth.Application.Commands;
using Core.Shared.Results;

namespace Auth.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var res = await _mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Auth.Application.DTOs.LoginRequest req)
        {
            var cmd = new LoginCommand(req.Email, req.Password);
            var res = await _mediator.Send(cmd);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }
    }
}
