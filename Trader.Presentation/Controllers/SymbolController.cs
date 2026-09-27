using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.Symbol;
using Trader.Application.Queries.Symbol;


namespace Trader.Presentation.Controller
{
    [ApiController]
    [Route("api/Trader/[controller]")]
    public class SymbolController : BaseController
    {
        /* ═══════════ Queries ═══════════ */

        [HttpGet("GetList")]
        [AuthorizeResource("trader.symbol", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetSymbolListQuery? request = null)
        {
            var result = await Mediator.Send(request ?? new GetSymbolListQuery());
            return HandleResult(result);
        }

        [HttpGet("GetSelectionList")]
        [AuthorizeResource("trader.symbol", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetSymbolsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request ?? new GetSymbolsSelectionListQuery());
            return HandleResult(result);
        }

        /* ═══════════ Commands ═══════════ */

        [HttpPost("Create")]
        [AuthorizeResource("trader.symbol", "Create")]
        public async Task<IActionResult> CreateSymbol([FromBody] CreateSymbolCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeResource("trader.symbol", "Edit")]
        public async Task<IActionResult> UpdateSymbol(Guid id, [FromBody] UpdateSymbolCommand command)
        {
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeResource("trader.symbol", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteSymbolCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /* ═══════════ Custom actions ═══════════ */

        [HttpPost("GetMarketInfo")]
        [AuthorizeResource("trader.symbol", "View")]
        public async Task<IActionResult> GetMarketInfo([FromBody] GetSymbolMarketInfoQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}