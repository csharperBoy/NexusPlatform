using Core.Application.Helper;
using Core.Application.Models;
using Core.Presentation.Controllers;
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
    public class SettingController : BaseController
    {
        [HttpGet("GetActiveModules")]
        //[AuthorizeResource("core.setting", "View")]
        public async Task<IActionResult> GetActiveModules()
        {
            Result<List<ModuleItem>> lst = new Result<List<ModuleItem>>(true, ModuleHelper.GetActiveModules());
            return HandleResult(lst);
        }
    }
}
