using Core.Application.Helper;
using Core.Application.Models;
using Core.Domain.Enums;
using Core.Presentation.Filters;
using Core.Shared.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Presentation.Controllers
{
    [ApiController]
    [Route("api/core/[controller]")]
    public class SettingController : BaseController
    {
        [HttpGet("GetActiveModules")]
        //[AuthorizeResource("core.setting", "View")]
        public async Task<IActionResult> GetActiveModules()
        {
            Result<List<ModuleItem>> lst = new Result<List<ModuleItem>>(true, ModuleHelper.GetActiveModules(), "");
            return HandleResult(lst);
        }
    }
}
