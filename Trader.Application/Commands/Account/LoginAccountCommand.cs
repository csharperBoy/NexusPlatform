using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Account
{
    public record LoginAccountCommand(Guid Id) : IRequest<Result<bool>>;

    public class LoginAccountCommandHandler
        : IRequestHandler<LoginAccountCommand, Result<bool>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<LoginAccountCommandHandler> _logger;

        public LoginAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<LoginAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            LoginAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Logging in TraderAccount: {Id}", request.Id);

                await _accountService.LoginAsync(request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to login TraderAccount: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}