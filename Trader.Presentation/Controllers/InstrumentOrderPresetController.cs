using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.InstrumentOrderPreset;
using Trader.Application.Dtos.Instruments;
using Trader.Application.Queries.InstrumentOrderPreset;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class InstrumentOrderPresetController : BaseController
{
    /* ═══════════ Queries ═══════════ */

    [HttpGet("GetByInstrument/{isin}")]
    [AuthorizeResource("trader.orderpreset", "View")]
    public async Task<IActionResult> GetByInstrument(string isin)
    {
        var result = await Mediator.Send(new GetOrderPresetByInstrumentQuery(isin));
        return HandleResult(result);
    }

    /* ═══════════ Commands ═══════════ */

    [HttpPost("Create")]
    [AuthorizeResource("trader.orderpreset", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateOrderPresetCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizeResource("trader.orderpreset", "Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderPresetModel model)
    {
        var command = new UpdateOrderPresetCommand(id, model);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeResource("trader.orderpreset", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteOrderPresetCommand(id));
        return HandleResult(result);
    }
}