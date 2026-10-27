using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Dtos.TradingEvents;
using Trader.Application.Queries.TradingEvents;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class TradingEventController : BaseController
{
    [HttpPost("Query")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> Query([FromBody] TradingEventQuery query)
    {
        var result = await Mediator.Send(new QueryTradingEventsQuery(query));
        return HandleResult(result);
    }
}