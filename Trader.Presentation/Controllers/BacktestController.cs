using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.Backtest;
using Trader.Application.Dtos.Backtests;
using Trader.Application.Queries.Backtest;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class BacktestController : BaseController
{
    /* ═══════════ Commands ═══════════ */

    [HttpPost("Run")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Run([FromBody] RunBacktestCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("Cancel/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelBacktestCommand(id));
        return HandleResult(result);
    }

    /* ═══════════ Queries ═══════════ */

    [HttpGet("GetList")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? sourceRunId = null,
        [FromQuery] int take = 100)
    {
        var result = await Mediator.Send(
            new GetBacktestListQuery(sourceRunId, take));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetBacktestByIdQuery(id));
        return HandleResult(result);
    }

    [HttpGet("GetTrades/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetTrades(Guid id)
    {
        var result = await Mediator.Send(new GetBacktestTradesQuery(id));
        return HandleResult(result);
    }

    [HttpGet("GetEquityCurve/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetEquityCurve(Guid id)
    {
        var result = await Mediator.Send(new GetBacktestEquityCurveQuery(id));
        return HandleResult(result);
    }

    [HttpPost("Compare")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> Compare([FromBody] List<Guid> ids)
    {
        var result = await Mediator.Send(new CompareBacktestsQuery(ids));
        return HandleResult(result);
    }
    [HttpPost("QueryEvents")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> QueryEvents([FromBody] BacktestEventQuery query)
    {
        var result = await Mediator.Send(new GetBacktestEventsQuery(query));
        return HandleResult(result);
    }
}