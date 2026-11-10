using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Commands.PriceCollection;
using Trader.Application.Queries.PriceCollection;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class PriceCollectionJobController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("trader.pricecollection", "View")]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? priceSourceId = null,
        [FromQuery] int take = 100)
    {
        var result = await Mediator.Send(
            new GetPriceCollectionJobListQuery(priceSourceId, take));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "View")]
    public async Task<IActionResult> GetById(
        Guid id, [FromServices] IPriceCollectionJobQueryService svc, CancellationToken ct)
    {
        var view = await svc.GetByIdAsync(id, ct);
        if (view is null) return NotFound();
        return Ok(view);
    }

    [HttpPost("Create")]
    [AuthorizeResource("trader.pricecollection", "Create")]
    public async Task<IActionResult> Create([FromBody] CreatePriceCollectionJobCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("Start/{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "Edit")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await Mediator.Send(new StartPriceCollectionJobCommand(id));
        return HandleResult(result);
    }

    [HttpPost("Stop/{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "Edit")]
    public async Task<IActionResult> Stop(Guid id)
    {
        var result = await Mediator.Send(new StopPriceCollectionJobCommand(id));
        return HandleResult(result);
    }
}