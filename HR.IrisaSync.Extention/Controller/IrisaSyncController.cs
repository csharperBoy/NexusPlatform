using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using HR.IrisaSync.Extention.Interface;
using HR.IrisaSync.Extention.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Controller
{
    [ApiController]
    [Route("api/HR/[controller]")]
    public class IrisaSyncController : BaseController
    {
        private readonly ISyncService _syncService;
        public IrisaSyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpGet("GetList")]
        //[AuthorizeResource("hr.orgchart", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetEmploymentQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpPost("SyncEmployement")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncEmployement()
        {
            await _syncService.SyncEmploymentsAsync();

            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillJobTitle")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillJobTitle()
        {
            await _syncService.SyncJobTitle();

            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillJobLevel")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillJobLevel()
        {
            await _syncService.SyncJobLevel();

            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillOrganizationUnit")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillOrganizationUnit()
        {
            await _syncService.SyncOrganizationUnit();

            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillPost")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillPost()
        {
            await _syncService.SyncPostAsync();

            return HandleResult(Result<bool>.Ok(true));
        }
    }

}
