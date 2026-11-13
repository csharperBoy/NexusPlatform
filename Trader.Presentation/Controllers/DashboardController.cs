using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.Dashboard;

namespace Trader.Presentation.Controller;

[ApiController]
[Route("api/Trader/[controller]")]
public class DashboardController : BaseController
{
    [HttpGet("Summary")]
    [AuthorizeResource("trader.dashboard", "View")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDashboardSummaryQuery(), ct);
        return HandleResult(result);
    }
}