using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contact.Application.Commands.Post;
using Contact.Application.Queries;

namespace Contact.Presentation.Controller
{
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class OrgChartController : BaseController
    {
        [HttpPut("{id:guid}")]
        [AuthorizeResource("contact.post", "Edit")]
        public async Task<IActionResult> UpdatePostContact(Guid id, [FromBody] UpdatePostContactCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPut("batch")]
        [AuthorizeResource("contact.post", "Edit")]
        public async Task<IActionResult> BatchUpdatePostContacts([FromBody] BatchUpdatePostsContactCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

       
        [HttpGet("GetList")]
        [AuthorizeResource("contact.post", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetPostContactListQuery request )
        {

            var result = await Mediator.Send(request);
            return HandleResult(result);
        }

      
    }

}
