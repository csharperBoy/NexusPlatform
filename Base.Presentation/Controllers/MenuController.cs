using Base.Application.Queries.Menu;
using Core.Application.Helper;
using Core.Application.Models;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Core.Shared.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Presentation.Controllers
{
    [ApiController]
    [Route("api/base/[controller]")]
    public class MenuController : BaseController
    {
        [HttpGet("GetMenuTreeQuery")]
        //[AuthorizeResource("base.menu", "View")]
        public async Task<IActionResult> GetMenuTreeQuery()
        {

            var query = new GetMenuTreeQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}
