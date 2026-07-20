using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using HR.Application.Commands.OrgChart;
using HR.Application.Queries.Post;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("Create")]
        //[AuthorizeResource("hr.post", "Create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        //[AuthorizeResource("hr.post", "Edit")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        [HttpGet("GetList")]
        //[AuthorizeResource("hr.post", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetPostListQuery? request = null)
        {
            var result = await Mediator.Send(request);
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
