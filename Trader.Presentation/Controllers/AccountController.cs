using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trader.Application.Commands.Account;
namespace Trader.Presentation.Controller
{
    [ApiController]
    [Route("api/Trader/[controller]")]
    public class AccountController : BaseController
    {
        /* ═══════════ Queries ═══════════ */

        [HttpGet("GetList")]
        [AuthorizeResource("trader.account", "View")]
        public async Task<IActionResult> GetList([FromQuery] GetAccountListQuery? request = null)
        {
            var result = await Mediator.Send(request ?? new GetAccountListQuery());
            return HandleResult(result);
        }

        [HttpGet("GetSelectionList")]
        [AuthorizeResource("trader.account", "View")]
        public async Task<IActionResult> GetSelectionList([FromQuery] GetAccountsSelectionListQuery? request = null)
        {
            var result = await Mediator.Send(request ?? new GetAccountsSelectionListQuery());
            return HandleResult(result);
        }

        /* ═══════════ Commands ═══════════ */

        [HttpPost("Create")]
        [AuthorizeResource("trader.account", "Create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeResource("trader.account", "Edit")]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
        {
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeResource("trader.account", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteAccountCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /* ═══════════ Custom actions ═══════════ */

        [HttpPost("Login")]
        [AuthorizeResource("trader.account", "Edit")]
        public async Task<IActionResult> Login([FromBody] LoginAccountCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("Activate")]
        [AuthorizeResource("trader.account", "Edit")]
        public async Task<IActionResult> Activate([FromBody] ActivateAccountCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}