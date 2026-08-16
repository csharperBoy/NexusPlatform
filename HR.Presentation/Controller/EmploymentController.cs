using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using HR.Application.Commands.Employment;
using HR.Application.Commands.OrgChart;
using HR.Application.Queries.Employment;
using HR.Application.Queries.Post;
using HR.Domain.Specifications;
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
    public class EmploymentController : BaseController
    {
        [HttpPost("Create")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> CreateEmployment([FromBody] CreateEmploymentCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        [AuthorizeResource("hr.employment", "Edit")]
        public async Task<IActionResult> UpdateEmployment(Guid id, [FromBody] UpdateEmploymentCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("hr.employment", "Edit")]
        public async Task<IActionResult> BatchUpdateemployments([FromBody] BatchUpdateEmploymentsCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("GetList")]
        [AuthorizeResource("hr.employment", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetEmploymentListQuery request)
        {


            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        

        [HttpGet("GetSelectionList")]
        [AuthorizeResource("hr.employment", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetEmploymentsSelectionListQuery? request = null)
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
        [HttpPut("{id:guid}")]
        [AuthorizeResource("hr.orgchart", "Edit")]
        public async Task<IActionResult> UpdateOrgChart(Guid id, [FromBody] UpdateOrgChartCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
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
