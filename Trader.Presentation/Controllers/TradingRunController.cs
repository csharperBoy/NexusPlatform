using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.TradingRun;
using Trader.Application.Queries.TradingRun;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class TradingRunController : BaseController
{
    /* ═══════════ Queries ═══════════ */

    [HttpGet("GetList")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetTradingRunListQuery());
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTradingRunByIdQuery(id));
        return HandleResult(result);
    }

    /* ═══════════ Commands ═══════════ */

    [HttpPost("Create")]
    [AuthorizeResource("trader.tradingrun", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateTradingRunCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTradingRunCommand command)
    {
        var updatedCommand = command with { Id = id };
        var result = await Mediator.Send(updatedCommand);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteTradingRunCommand(id));
        return HandleResult(result);
    }

    /* ═══════════ Custom Actions ═══════════ */

    [HttpPost("Start/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await Mediator.Send(new StartTradingRunCommand(id));
        return HandleResult(result);
    }

    [HttpPost("Stop/{id:guid}")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Stop(Guid id, [FromQuery] int behavior = 0)
    {
        var result = await Mediator.Send(new StopTradingRunCommand(id, behavior));
        return HandleResult(result);
    }
}