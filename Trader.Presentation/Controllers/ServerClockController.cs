using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using Trader.Application.Commands.ServerClock;

namespace Trader.Presentation.Controller
{
    [ApiController]
    [Route("api/Trader/[controller]")]
    public class ServerClockController : BaseController
    {
        /* ═══════════ Queries ═══════════ */

        [HttpGet("GetStatus")]
        [AuthorizeResource("trader.serverclock", "View")]
        public async Task<IActionResult> GetStatus()
        {
            var query = new GetServerClockStatusQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /* ═══════════ Commands ═══════════ */

        [HttpPost("Sync")]
        [AuthorizeResource("trader.serverclock", "Edit")]
        public async Task<IActionResult> Sync([FromBody] SyncServerClockCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}