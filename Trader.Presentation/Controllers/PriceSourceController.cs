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
public class PriceSourceController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("trader.pricecollection", "View")]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetPriceSourceListQuery());
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "View")]
    public async Task<IActionResult> GetById(
        Guid id, [FromServices] IPriceSourceQueryService svc, CancellationToken ct)
    {
        var view = await svc.GetByIdAsync(id, ct);
        if (view is null) return NotFound();
        return Ok(view);
    }

    [HttpPost("Create")]
    [AuthorizeResource("trader.pricecollection", "Create")]
    public async Task<IActionResult> Create([FromBody] CreatePriceSourceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceSourceCommand command)
    {
        var updated = command with { Id = id };
        var result = await Mediator.Send(updated);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeResource("trader.pricecollection", "Delete")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeletePriceSourceCommand(id));
        return HandleResult(result);
    }
}