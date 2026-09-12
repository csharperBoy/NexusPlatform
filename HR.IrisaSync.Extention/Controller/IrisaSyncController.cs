using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using HR.Application.Commands.Assignment;
using HR.Application.Commands.Employment;
using HR.Application.Commands.OrgChart;
using HR.IrisaSync.Extention.Commands;
using HR.IrisaSync.Extention.Commands.JobLevel;
using HR.IrisaSync.Extention.Commands.JobTitle;
using HR.IrisaSync.Extention.Commands.OrganizationUnit;
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
      
        [HttpGet("SyncEmployement")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncEmployement()
        {
           var result = await _syncService.SyncEmploymentsAsync();

            return HandleBatchResult(result);

        }
      
        [HttpGet("SyncEmploymentsPreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncEmploymentsPreview()
        {
           var result = await _syncService.SyncEmploymentsPreviewAsync();

            return HandleBatchResult(result);

        }
      
        [HttpPost("ApplyEmployments")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyEmployments([FromBody] SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand> selectedBundle)
        {
           var result = await _syncService.ApplyEmploymentsAsync(selectedBundle);

            return HandleBatchResult(result);

        }
        [HttpGet("SyncJobTitlePreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncJobTitlePreview()
        {
            var result = await _syncService.SyncJobTitlePreviewAsync();

            return HandleBatchResult(result);
        }
        [HttpPost("ApplyJobTitle")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyJobTitle([FromBody] SyncCommandBundle<CreateJobTitleIrisaSyncCommand, UpdateJobTitleIrisaSyncCommand, DeleteJobTitleIrisaSyncCommand> selectedBundle)
        {
            var result = await _syncService.ApplyJobTitleAsync(selectedBundle);

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
        [HttpGet("SyncJobLevelPreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncJobLevelPreview()
        {
            var result = await _syncService.SyncJobLevelPreviewAsync();

            return HandleBatchResult(result);
        }
        [HttpPost("ApplyJobLevel")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyJobLevel([FromBody] SyncCommandBundle<CreateJobLevelIrisaSyncCommand, UpdateJobLevelIrisaSyncCommand, DeleteJobLevelIrisaSyncCommand> selectedBundle)
        {
            var result = await _syncService.ApplyJobLevelAsync(selectedBundle);

            return HandleBatchResult(result);
        }
        [HttpGet("SyncOrganizationUnit")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncOrganizationUnit()
        {
            var result = await _syncService.SyncOrganizationUnitAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncOrganizationUnitPreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncOrganizationUnitPreview()
        {
            var result = await _syncService.SyncOrganizationUnitPreviewAsync();

            return HandleBatchResult(result);
        }
        [HttpPost("ApplyOrganizationUnit")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyOrganizationUnit([FromBody] SyncCommandBundle<CreateOrganizationUnitIrisaSyncCommand, UpdateOrganizationUnitIrisaSyncCommand, DeleteOrganizationUnitIrisaSyncCommand> selectedBundle)
        {
            var result = await _syncService.ApplyOrganizationUnitAsync(selectedBundle);

            return HandleBatchResult(result);
        }
        [HttpGet("SyncPost")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncPost()
        {
            var result = await _syncService.SyncPostAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncPostPreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncPostPreview()
        {
            var result = await _syncService.SyncPostPreviewAsync();

            return HandleBatchResult(result);
        }
        [HttpPost("ApplyPost")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyPost([FromBody] SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand> selectedBundle)
        {
            var result = await _syncService.ApplyPostAsync(selectedBundle);

            return HandleBatchResult(result);
        }
        [HttpGet("SyncAssignments")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncAssignments()
        {
            var result = await _syncService.SyncAssignmentsAsync();

            return HandleBatchResult(result);
        }
        [HttpGet("SyncAssignmentsPreview")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> SyncAssignmentsPreview()
        {
            var result = await _syncService.SyncAssignmentsPreviewAsync();

            return HandleBatchResult(result);
        }
        [HttpPost("ApplyAssignments")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> ApplyAssignments([FromBody] SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand> selectedBundle)
        {
            var result = await _syncService.ApplyAssignmentsAsync(selectedBundle);

            return HandleBatchResult(result);
        }

    }

}
