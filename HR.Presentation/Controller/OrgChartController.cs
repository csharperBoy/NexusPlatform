using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using HR.Application.Commands.OrgChart;
using HR.Application.Interfaces;
using HR.Application.Queries.Post;
using HR.Domain.Entities;
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

        private readonly IOrgChartInternalService _orgChartInternalService;
        public OrgChartController(IOrgChartInternalService orgChartInternalService)
        {
            _orgChartInternalService = orgChartInternalService;
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
