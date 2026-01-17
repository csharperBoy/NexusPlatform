using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Trader.Server.Collector.Presentation.Controllers
{
    [ApiController]
    [Route("api/Collector/[controller]")]
    public class CollectorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CollectorController(IMediator mediator)
        {
            _mediator = mediator;
        }

       
    }
}
