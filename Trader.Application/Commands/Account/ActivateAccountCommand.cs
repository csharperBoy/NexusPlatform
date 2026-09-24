using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Account
{
    public record ActivateAccountCommand(Guid Id) : IRequest<Result<bool>>;

    public class ActivateAccountCommandHandler
        : IRequestHandler<ActivateAccountCommand, Result<bool>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<ActivateAccountCommandHandler> _logger;

        public ActivateAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<ActivateAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            ActivateAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Activating TraderAccount token: {Id}", request.Id);

                var ok = await _accountService.ActivateAsync(request.Id);

                if (!ok)
                    return Result<bool>.Fail("Activation failed.");

                _logger.LogInformation(
                    "TraderAccount token activated: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to activate TraderAccount: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}