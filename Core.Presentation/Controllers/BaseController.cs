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
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            if (result.Data == null)
            {
                return NoContent();
            }

            return Ok(result.Data);
        }

        protected IActionResult HandleResult(Result result)
        {
            return result.Succeeded ? Ok() : BadRequest(result.Error);
        }
    }
}
