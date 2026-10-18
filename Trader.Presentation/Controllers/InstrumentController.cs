using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.Instrument;
using Trader.Application.Queries.Instrument;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class InstrumentController : BaseController
{
    /* ═══════════ Queries ═══════════ */

    [HttpGet("GetList")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetInstrumentListQuery());
        return HandleResult(result);
    }

    [HttpGet("GetSelectionList")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetSelectionList()
    {
        var result = await Mediator.Send(new GetInstrumentSelectionListQuery());
        return HandleResult(result);
    }

    /* ═══════════ Commands ═══════════ */

    [HttpPost("Create")]
    [AuthorizeResource("trader.instrument", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateInstrumentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizeResource("trader.instrument", "Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInstrumentCommand command)
    {
        var updatedCommand = command with { Id = id };
        var result = await Mediator.Send(updatedCommand);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeResource("trader.instrument", "Delete")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateInstrumentCommand(id));
        return HandleResult(result);
    }
}