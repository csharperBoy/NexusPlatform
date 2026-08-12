using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Contact.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Presentation.Controllers
{
    [ApiController]
    [Route("api/Contact/[controller]")]
    public class PhoneBookController : BaseController
    {

        [HttpGet("GetList")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetPhoneBookListQuery? request = null)
        {
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }



    }


}
