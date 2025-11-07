using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Application.Commands;
using Sample.Application.Interfaces;

namespace Sample.Presentation.Controllers
{
    [ApiController]
    [Route("api/sample/[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SampleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("SampleApi")]
        public async Task<IActionResult> SampleApi([FromBody] SampleApiCommand command)
        {
            var res = await _mediator.Send(command);
            if (!res.Succeeded) return BadRequest(res.Error);
            return Ok(res.Data);
        }
 }
}
