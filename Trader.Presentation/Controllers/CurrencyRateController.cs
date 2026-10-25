using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.CurrencyRate;
using Trader.Application.Queries.CurrencyRate;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class CurrencyRateController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetList(
        [FromQuery] string? fromCurrency = null,
        [FromQuery] string? toCurrency = null,
        [FromQuery] int limit = 100)
    {
        var result = await Mediator.Send(
            new GetCurrencyRateListQuery(fromCurrency, toCurrency, limit));
        return HandleResult(result);
    }

    [HttpGet("GetLatest")]
    [AuthorizeResource("trader.instrument", "View")]
    public async Task<IActionResult> GetLatest(
        [FromQuery] string fromCurrency,
        [FromQuery] string toCurrency,
        [FromQuery] DateTime? atTime = null)
    {
        var result = await Mediator.Send(
            new GetLatestCurrencyRateQuery(fromCurrency, toCurrency, atTime));
        return HandleResult(result);
    }

    [HttpPost("Create")]
    [AuthorizeResource("trader.instrument", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyRateCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}