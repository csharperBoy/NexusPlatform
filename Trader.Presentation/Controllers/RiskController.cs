using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Commands.Risk;
using Trader.Application.Queries.Risk;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class RiskController : BaseController
{
    [HttpPost("CircuitBreaker/Reset")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> ResetCircuitBreaker(
    [FromBody] ResetCircuitBreakerCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("CircuitBreaker/Active")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> GetActiveCircuits(
        [FromServices] IRunAccountLossStreakTracker tracker,
        CancellationToken ct)
    {
        var active = await tracker.GetActiveCircuitsAsync(ct);
        return Ok(active);
    }
    /* ═══════════ Kill Switch ═══════════ */

    [HttpPost("Kill-Switch/Activate")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Activate([FromBody] ActivateKillSwitchCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("Kill-Switch/Deactivate")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateKillSwitchCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("Kill-Switch/Status")]
    [AuthorizeResource("trader.tradingrun", "View")]
    public async Task<IActionResult> Status()
    {
        var result = await Mediator.Send(new GetKillSwitchStatusQuery());
        return HandleResult(result);
    }
}