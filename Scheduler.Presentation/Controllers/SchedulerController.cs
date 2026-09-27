using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Presentation.Controllers
{
    
    [ApiController]
    [Route("api/Scheduler/[controller]")]
    public class SchedulerController : BaseController
    {
        private readonly ISchedulerService _scheduler;

        public SchedulerController(ISchedulerService scheduler)
            => _scheduler = scheduler;

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ScheduledJobInfo>> Get(Guid id, CancellationToken ct)
        {
            var job = await _scheduler.GetAsync(id, ct);
            return job is null ? NotFound() : Ok(job);
        }

        [HttpPost("query")]
        public async Task<ActionResult<IReadOnlyList<ScheduledJobInfo>>> Query(
            [FromBody] SchedulerQuery query, CancellationToken ct)
        {
            var result = await _scheduler.QueryAsync(query, ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Cancel(Guid id, CancellationToken ct)
        {
            var ok = await _scheduler.CancelAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

    }
}
