using Contact.Application.Commands.Employment;
using Contact.Application.Queries;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Presentation.Controllers
{
    
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class ContactResourceController : BaseController
    {

        //[HttpPut("{id:guid}")]
        //[AuthorizeResource("contact.contactresource", "Edit")]
        //public async Task<IActionResult> UpdateEmployment(Guid id, [FromBody] UpdateContactResourceCommand command)
        //{
        //    // اطمینان از تطابق ID در route با command
        //    var updatedCommand = command with { Id = id };
        //    var result = await Mediator.Send(updatedCommand);
        //    return HandleResult(result);
        //}
        //[HttpPut("batch")]
        //[AuthorizeResource("contact.contactresource", "Edit")]
        //public async Task<IActionResult> BatchUpdate([FromBody] BatchUpdateContactResourceCommand command)
        //{
        //    var result = await Mediator.Send(command);
        //    return HandleResult(result);
        //}
        //[HttpGet("GetList")]
        //[AuthorizeResource("contact.contactresource", "View")]
        //public async Task<IActionResult> GetList([FromQuery] GetContactResourceListQuery request)
        //{


        //    var result = await Mediator.Send(request);
        //    return HandleResult(result);
        //}

    }
}
