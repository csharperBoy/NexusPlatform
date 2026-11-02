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
    [HttpPost("Collect")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Collect(
        [FromBody] CollectHistoricalDataCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("GetList")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetList([FromQuery] int take = 50)
    {
        var result = await Mediator.Send(
            new GetHistoricalDataCollectionJobsQuery(take));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(
            new GetHistoricalDataCollectionJobByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost("Cancel/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(
            new CancelHistoricalDataCollectionJobCommand(id));
        return HandleResult(result);
    }
}