using Core.Presentation.Controllers;
using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TraderServer.Presentation.Controllers
{
    [ApiController]
    [Route("api/Collector/[controller]")]
    public class CollectorController : BaseController
    {
        private readonly IMediator _mediator;

        public CollectorController(IMediator mediator)
        {
            _mediator = mediator;
        }

       
    }
}
