using Contact.Application.Commands.ContactResource;
using Contact.Application.Commands.Employment;
using Contact.Application.Queries;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Presentation.Controllers
{
    
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class ContactResourceController : BaseController
    {

        [HttpPost("Create")]
        //[AuthorizeResource("contact.contactresource", "Create")]
        public async Task<IActionResult> CreateContactResource([FromBody] CreateContactResourceCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        [AuthorizeResource("contact.contactresource", "Edit")]
        public async Task<IActionResult> UpdateContactResource(Guid id, [FromBody] UpdateContactResourceCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("contact.contactresource", "Edit")]
        public async Task<IActionResult> BatchUpdatecontactResources([FromBody] BatchUpdateContactResourcesCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("GetList")]
        [AuthorizeResource("contact.contactresource", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetContactResourceListQuery request)
        {


            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("GetSelectionList")]
        [AuthorizeResource("contact.contactresource", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetContactResourcesSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("contact.contactresource", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteContactResourceCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

    }
}
