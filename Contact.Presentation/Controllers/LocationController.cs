using Contact.Application.Commands.Location;
using Contact.Application.Queries;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Presentation.Controller
{
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class LocationController : BaseController
    {
     
        [HttpPut("{id:guid}")]
        [AuthorizeResource("contact.location", "Edit")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationContactCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("contact.location", "Edit")]
        public async Task<IActionResult> BatchUpdatelocations([FromBody] BatchUpdateLocationsContactCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("GetList")]
        [AuthorizeResource("contact.location", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetLocationContactListQuery request)
        {


            var result = await Mediator.Send(request);
            return HandleResult(result);
        }

    }
}
