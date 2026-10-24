using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.TradingSchedule;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class TradingScheduleController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetTradingScheduleListQuery());
        return HandleResult(result);
    }

    [HttpGet("GetSelectionList")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetSelectionList()
    {
        var result = await Mediator.Send(new GetTradingScheduleSelectionListQuery());
        return HandleResult(result);
    }
}