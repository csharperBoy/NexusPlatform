using Authorization.Application.Interfaces;
using Authorization.Application.Provider;
using Authorization.Application.Queries.Permissions;
using Authorization.Application.Queries.Resource;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/authorization/admin/[controller]")]
    public class ResourceMetadataController : BaseController
    {

        
        [HttpGet("metadata/{resourceKey}")]
        //[AuthorizeResource("authorization.resource", "View")]
        public async Task<IActionResult> Get( string? resourceKey)//[FromQuery] GetResourceMetaDataQuery? request = null)
        {
            GetResourceMetaDataQuery request = new GetResourceMetaDataQuery(resourceKey);
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
    }
}
