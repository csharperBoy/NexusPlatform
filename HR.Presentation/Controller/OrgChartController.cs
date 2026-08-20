using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using HR.Application.Commands.Employment;
using HR.Application.Commands.OrgChart;
using HR.Application.Interfaces;
using HR.Application.Queries.CostCenter;
using HR.Application.Queries.Post;
using HR.Domain.Entities;
using HR.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Presentation.Controller
{
    [ApiController]
    [Route("api/HR/[controller]")]
    public class OrgChartController : BaseController
    {

        #region metadata
        [HttpGet("JobTitle/GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetJobTitleSelectionList([FromQuery] GetJobTitlesSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("OrganizationUnit/GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetOrganizationUnitSelectionList([FromQuery] GetOrganizationUnitsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("JobLevel/GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetJobLevelSelectionList([FromQuery] GetJobLevelsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("Grade/GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetGradeSelectionList([FromQuery] GetGradesSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("CostCenter/GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetCostCenterSelectionList([FromQuery] GetCostCentersSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
       
        #endregion

        [HttpPost("Create")]
        //[AuthorizeResource("hr.post", "Create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        [AuthorizeResource("hr.post", "Edit")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("hr.post", "Edit")]
        public async Task<IActionResult> BatchUpdatePosts([FromBody] BatchUpdatePostsCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

       
        [HttpGet("GetList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetPostListQuery request )
        {

            //var posts =await _orgChartInternalService.GetPostListAsync();
            //var a = posts.ToList();
            //var result = Result<IReadOnlyList<PostInfoView>>.Ok(posts);
         

            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("GetSelectionList")]
        [AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetPostsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("hr.post", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePostCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        /*
        [HttpGet("{id:guid}")]
        [AuthorizeResource("hr.orgchart", "View")]
        public async Task<IActionResult> GetOrgChartById(Guid id)
        {
            var query = new GetOrgChartByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("GetSelectionList")]
        [AuthorizeResource("hr.orgchart", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetOrgChartsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }

        
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("hr.orgchart", "Delete")]
        public async Task<IActionResult> DeleteOrgChart(Guid id)
        {
            var command = new DeleteOrgChartCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        */
    }

}
