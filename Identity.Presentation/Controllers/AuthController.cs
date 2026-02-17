using Identity.Application.Commands;
using Identity.Infrastructure.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions.Value;
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // حتما روی HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private string? GetRefreshTokenFromCookie()
        {
            Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            return refreshToken;
        }

        private void RemoveRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var res = await _mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);

            SetRefreshTokenCookie(res.Data.RefreshToken);

            return Ok(new
            {
                res.Data.AccessToken,
                res.Data.UserId,
                res.Data.UserName,
                res.Data.ExpireAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand req)
        {
           var res = await _mediator.Send(req);
            if (!res.Succeeded) return BadRequest(res.Error);

            SetRefreshTokenCookie(res.Data.RefreshToken);

            return Ok(new
            {
                res.Data.AccessToken,
                res.Data.UserId,
                res.Data.UserName,
                res.Data.ExpireAt
            });
        }

       
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized("Refresh token not found.");

            var res = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            if (!res.Succeeded) return BadRequest(res.Error);

            SetRefreshTokenCookie(res.Data.RefreshToken);

            return Ok(new
            {
                res.Data.AccessToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _mediator.Send(new LogoutCommand(refreshToken));
            }

            RemoveRefreshTokenCookie();
            return Ok();
        }
    }
}