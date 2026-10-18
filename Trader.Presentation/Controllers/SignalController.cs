using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.Signal;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class SignalController : BaseController
{
    [HttpGet("GetRecentByRun/{runId:guid}")]
    [AuthorizeResource("trader.signal", "View")]
    public async Task<IActionResult> GetRecentByRun(
        Guid runId, [FromQuery] int take = 100)
    {
        var result = await Mediator.Send(new GetRecentSignalsQuery(runId, take));
        return HandleResult(result);
    }
}