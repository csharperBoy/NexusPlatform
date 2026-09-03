using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using HR.IrisaSync.Extention.Commands;
using HR.IrisaSync.Extention.Interface;
using HR.IrisaSync.Extention.Queries;
using HR.IrisaSync.Extention.Services;
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

        [HttpGet("syncWithIrisa")]
        [AuthorizeResource("hr.irisasync", "View")]
        public async Task<IActionResult> SyncWithIrisa([FromQuery] SyncWithIrisaCommand? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        /*
        [HttpGet("GetList")]
        //[AuthorizeResource("hr.orgchart", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetEmploymentQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        */
        [HttpGet("SyncEmployement")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncEmployement()
        {
           var result = await _syncService.SyncEmploymentsAsync();

            return HandleBatchResult(result);

        }
        [HttpGet("SyncJobTitle")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncJobTitle()
        {
            var result = await _syncService.SyncJobTitleAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncJobLevel")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncJobLevel()
        {
            var result = await _syncService.SyncJobLevelAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncOrganizationUnit")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncOrganizationUnit()
        {
            var result = await _syncService.SyncOrganizationUnitAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncPost")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncPost()
        {
            var result = await _syncService.SyncPostAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncAssignments")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncAssignments()
        {
            var result = await _syncService.SyncAssignmentsAsync();

            return HandleBatchResult(result);
        }

    }

}
