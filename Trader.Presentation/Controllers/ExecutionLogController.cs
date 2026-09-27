using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Queries.ExecutionLog;

namespace Trader.Presentation.Controller
{
    [ApiController]
    [Route("api/Trader/[controller]")]
    public class ExecutionLogController : BaseController
    {
        [HttpGet("GetLogs")]
        [AuthorizeResource("trader.executionlog", "View")]
        public async Task<IActionResult> GetLogs([FromQuery] GetExecutionLogsQuery query )
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}