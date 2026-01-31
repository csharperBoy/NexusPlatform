using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Application.Commands;

namespace TraderServer.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : BaseController
    {
        private readonly IMediator _mediator;

        public StockController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("add")]
        [AuthorizeResource("collector.stock", "Create")]
        public async Task<IActionResult> Add([FromBody] AddStockCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}
