using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Presentation.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? _mediator;
        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                if (result.Value == null)
                    return NoContent();

                return Ok(result.Value);
            }

            return BadRequest(new
            {
                success = false,
                error = result.Error,
                errors = result.Errors
            });
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }

            return BadRequest(new
            {
                success = false,
                error = result.Error,
                errors = result.Errors
            });
        }

        protected Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
