using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using People.Application.Commands.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Presentation.Controllers
{
    [ApiController]
    [Route("api/People/[controller]")]
    public class PersonController : BaseController
    {
        [HttpPost("Create")]
        [AuthorizeResource("people.person", "Create")]
        public async Task<IActionResult> CreateResource([FromBody] CreatePersonCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
/*
        [HttpGet("{id:guid}")]
        [AuthorizeResource("people.person", "View")]
        public async Task<IActionResult> GetPersonById(Guid id)
        {
            var query = new GetPersonByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("GetSelectionList")]
        [AuthorizeResource("people.person", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetPersonsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeResource("people.person", "Edit")]
        public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] UpdatePersonCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeResource("people.person", "Delete")]
        public async Task<IActionResult> DeletePerson(Guid id)
        {
            var command = new DeletePersonCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
*/
    }

}
