using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.HistoricalData;
using Trader.Application.Queries.HistoricalData;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class HistoricalDataCollectionController : BaseController
{
    [HttpPost("Backfill")]
    [AuthorizeResource("trader.historicaldata", "Edit")]
    public async Task<IActionResult> Backfill(
        [FromBody] CreateHistoricalBackfillCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("Recurring")]
    [AuthorizeResource("trader.historicaldata", "Edit")]
    public async Task<IActionResult> Recurring(
        [FromBody] CreateHistoricalRecurringCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("Start/{id:guid}")]
    [AuthorizeResource("trader.historicaldata", "Edit")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await Mediator.Send(new StartHistoricalJobCommand(id));
        return HandleResult(result);
    }

    [HttpPost("Stop/{id:guid}")]
    [AuthorizeResource("trader.historicaldata", "Edit")]
    public async Task<IActionResult> Stop(Guid id)
    {
        var result = await Mediator.Send(new StopHistoricalJobCommand(id));
        return HandleResult(result);
    }

    [HttpGet("GetList")]
    [AuthorizeResource("trader.historicaldata", "View")]
    public async Task<IActionResult> GetList([FromQuery] int take = 50)
    {
        var result = await Mediator.Send(
            new GetHistoricalDataCollectionJobsQuery(take));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.historicaldata", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(
            new GetHistoricalDataCollectionJobByIdQuery(id));
        return HandleResult(result);
    }
}