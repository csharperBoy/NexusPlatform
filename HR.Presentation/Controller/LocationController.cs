using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using HR.Application.Commands.Location;
using HR.Application.Queries.Location;
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
    public class LocationController : BaseController
    {
        [HttpPost("Create")]
        //[AuthorizeResource("hr.location", "Create")]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        [AuthorizeResource("hr.location", "Edit")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("hr.location", "Edit")]
        public async Task<IActionResult> BatchUpdatelocations([FromBody] BatchUpdateLocationsCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("GetList")]
        [AuthorizeResource("hr.location", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetLocationListQuery request)
        {


            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
       
    }
}
