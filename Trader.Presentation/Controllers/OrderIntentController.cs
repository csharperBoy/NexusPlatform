using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Dtos.Orders;
using Trader.Application.Queries.Orders;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class OrderIntentController : BaseController
{
    [HttpPost("Query")]
    [AuthorizeResource("trader.order", "View")]
    public async Task<IActionResult> Query([FromBody] OrderIntentQuery query)
    {
        var result = await Mediator.Send(new QueryOrderIntentsQuery(query));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.order", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetOrderIntentByIdQuery(id));
        return HandleResult(result);
    }
}