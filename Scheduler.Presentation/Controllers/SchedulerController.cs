using Core.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.Commands.RecurringSchedules;
using Scheduler.Application.Commands.ScheduledJobs;
using Scheduler.Application.Models;
using Scheduler.Application.Queries;
using Scheduler.Application.Queries.RecurringSchedules;
using Scheduler.Application.Queries.ScheduledJobs;

namespace Scheduler.Presentation.Controllers;

[ApiController]
[Route("api/Scheduler/[controller]")]
public class SchedulerController : BaseController
{
    /* ═══════════ Scheduled Jobs (One-Off + Occurrences) ═══════════ */

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await Mediator.Send(new GetScheduledJobQuery(id));
        return HandleResult(result);
    }

    [HttpPost("Query")]
    public async Task<IActionResult> Query([FromBody] SchedulerQuery request)
    {
        var result = await Mediator.Send(new QueryScheduledJobsQuery(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelScheduledJobCommand(id));
        return HandleResult(result);
    }

    /* ═══════════ Recurring Schedules ═══════════ */

    [HttpPost("Recurring/Create")]
    public async Task<IActionResult> CreateRecurring(
        [FromBody] CreateRecurringScheduleCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("Recurring/{id:guid}")]
    public async Task<IActionResult> GetRecurring(Guid id)
    {
        var result = await Mediator.Send(new GetRecurringScheduleQuery(id));
        return HandleResult(result);
    }

    [HttpPost("Recurring/Query")]
    public async Task<IActionResult> QueryRecurring(
        [FromBody] RecurringSchedulerQuery request)
    {
        var result = await Mediator.Send(new QueryRecurringSchedulesQuery(request));
        return HandleResult(result);
    }
}