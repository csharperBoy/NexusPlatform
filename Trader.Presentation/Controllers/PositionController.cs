using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.Position;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class PositionController : BaseController
{
    [HttpGet("GetByRun/{runId:guid}")]
    [AuthorizeResource("trader.position", "View")]
    public async Task<IActionResult> GetByRun(Guid runId)
    {
        var result = await Mediator.Send(new GetRunPositionsQuery(runId));
        return HandleResult(result);
    }

    [HttpGet("GetByAccount/{accountId:guid}")]
    [AuthorizeResource("trader.position", "View")]
    public async Task<IActionResult> GetByAccount(Guid accountId)
    {
        var result = await Mediator.Send(new GetAccountPositionsQuery(accountId));
        return HandleResult(result);
    }
}