using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.Strategy;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class StrategyController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("trader.strategy", "View")]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetStrategyListQuery());
        return HandleResult(result);
    }
}