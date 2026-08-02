using Core.Presentation.Controllers;
using Core.Shared.Results;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Controller
{
    [ApiController]
    [Route("api/HR/[controller]")]
    public class MapController : BaseController
    {
        private readonly IMapService _mapService;
        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }
        [HttpPost("FillJobTitleMap")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillJobTitleMap()
        {
            await _mapService.FillJobTitleMap();
            
            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillJobLevelMap")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillJobLevelMap()
        {
            await _mapService.FillJobLevelMap();
            
            return HandleResult(Result<bool>.Ok(true));
        }
        [HttpPost("FillOrganizationUnitMap")]
        //[AuthorizeResource("hr.employment", "Create")]
        public async Task<IActionResult> FillOrganizationUnitMap()
        {
            await _mapService.FillOrganizationUnitMap();
            
            return HandleResult(Result<bool>.Ok(true));
        }

    }
}
