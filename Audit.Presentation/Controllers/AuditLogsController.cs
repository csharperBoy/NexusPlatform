using Audit.Application.Interfaces;
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
        private readonly IAuditQueryService _auditQueryService;

        public AuditLogsController(IAuditQueryService auditQueryService)
        {
            _auditQueryService = auditQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditQueryService.GetRecentLogsAsync(100);
            return Ok(logs);
        }
    }
}
