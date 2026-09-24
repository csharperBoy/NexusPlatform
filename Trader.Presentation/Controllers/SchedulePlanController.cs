using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.SchedulePlan;
using Trader.Application.Queries.SchedulePlan;

namespace Trader.Presentation.Controller
{
    [ApiController]
    [Route("api/Trader/[controller]")]
    public class SchedulePlanController : BaseController
    {
        /* ═══════════ Queries ═══════════ */

        [HttpGet("GetList")]
        [AuthorizeResource("trader.scheduleplan", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetSchedulePlanListQuery? request = null)
        {
            var result = await Mediator.Send(request ?? new GetSchedulePlanListQuery());
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        [AuthorizeResource("trader.scheduleplan", "View")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetSchedulePlanByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /* ═══════════ Commands ═══════════ */

        [HttpPost("Create")]
        [AuthorizeResource("trader.scheduleplan", "Create")]
        public async Task<IActionResult> CreateSchedulePlan([FromBody] CreateSchedulePlanCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeResource("trader.scheduleplan", "Edit")]
        public async Task<IActionResult> UpdateSchedulePlan(Guid id, [FromBody] UpdateSchedulePlanCommand command)
        {
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeResource("trader.scheduleplan", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteSchedulePlanCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /* ═══════════ Custom actions ═══════════ */

        [HttpPost("Enable")]
        [AuthorizeResource("trader.scheduleplan", "Edit")]
        public async Task<IActionResult> Enable([FromBody] EnableSchedulePlanCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("Disable")]
        [AuthorizeResource("trader.scheduleplan", "Edit")]
        public async Task<IActionResult> Disable([FromBody] DisableSchedulePlanCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}