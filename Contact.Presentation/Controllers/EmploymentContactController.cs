using Contact.Application.Commands.Employment;
using Contact.Application.Queries;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Presentation.Controller
{
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class EmploymentContactController : BaseController
    {
        
        [HttpPut("{id:guid}")]
        [AuthorizeResource("contact.employment", "Edit")]
        public async Task<IActionResult> UpdateEmployment(Guid id, [FromBody] UpdateEmploymentContactCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("contact.employment", "Edit")]
        public async Task<IActionResult> BatchUpdateemployments([FromBody] BatchUpdateEmploymentsContactCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("GetList")]
        [AuthorizeResource("contact.employment", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetEmploymentContactListQuery request)
        {


            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
       
    }

}
