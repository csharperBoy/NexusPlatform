using Audit.Application.Interfaces;
using Audit.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuditLogsController(IMediator mediator) => _mediator = mediator;


        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IMediator mediator)
        {
            var logs = await _mediator.Send(new GetRecentAuditLogsQuery(100));
            return Ok(logs);
        }

    }
}
