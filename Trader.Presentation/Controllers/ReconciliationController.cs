using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.Reconciliation;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class ReconciliationController : BaseController
{
    [HttpPost("Run")]
    [AuthorizeResource("trader.tradingrun", "Edit")]
    public async Task<IActionResult> Run()
    {
        var result = await Mediator.Send(new RunReconciliationCommand());
        return HandleResult(result);
    }
}