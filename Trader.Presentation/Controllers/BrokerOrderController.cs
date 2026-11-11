using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Dtos.Orders;
using Trader.Application.Queries.Orders;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class BrokerOrderController : BaseController
{
    [HttpPost("Query")]
    [AuthorizeResource("trader.order", "View")]
    public async Task<IActionResult> Query([FromBody] BrokerOrderQuery query)
    {
        var result = await Mediator.Send(new QueryBrokerOrdersQuery(query));
        return HandleResult(result);
    }

    [HttpGet("GetById/{id:guid}")]
    [AuthorizeResource("trader.order", "View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetBrokerOrderByIdQuery(id));
        return HandleResult(result);
    }
}