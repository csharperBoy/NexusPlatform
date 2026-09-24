using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Account
{
    public record UpdateAccountCommand(
        Guid Id,
        string Name,
        string Username,
        string? Password
    ) : IRequest<Result<Guid>>;

    public class UpdateAccountCommandHandler
        : IRequestHandler<UpdateAccountCommand, Result<Guid>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<UpdateAccountCommandHandler> _logger;

        public UpdateAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<UpdateAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            UpdateAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating TraderAccount: {Id}", request.Id);

                var id = await _accountService.UpdateAccountAsync(
                    request.Id,
                    request.Name,
                    request.Username,
                    request.Password);

                await _accountService.SaveAsync();

                _logger.LogInformation(
                    "TraderAccount updated: {Id}", id);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update TraderAccount: {Id}", request.Id);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}