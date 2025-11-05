using Core.Shared.Results;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public AuthController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var res = await _mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }

        [HttpPost("login/username")]
        public async Task<IActionResult> LoginUsernameBase([FromBody] LoginRequest req)
        {
            var cmd = new LoginUsernameBaseCommand(req.UserIdentifier, req.Password);
            var res = await _mediator.Send(cmd);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }

        [HttpPost("login/email")]
        public async Task<IActionResult> LoginEmailBase([FromBody] LoginRequest req)
        {
            var cmd = new LoginEmailBaseCommand(req.UserIdentifier, req.Password);
            var res = await _mediator.Send(cmd);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var res = await _authService.RefreshTokenAsync(request);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var res = await _authService.LogoutAsync(request);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok();
        }
    }
}
