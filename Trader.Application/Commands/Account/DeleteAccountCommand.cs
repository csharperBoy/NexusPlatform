using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Account
{
    public record DeleteAccountCommand(Guid Id) : IRequest<Result<bool>>;

    public class DeleteAccountCommandHandler
        : IRequestHandler<DeleteAccountCommand, Result<bool>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<DeleteAccountCommandHandler> _logger;

        public DeleteAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<DeleteAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting TraderAccount: {Id}", request.Id);

                await _accountService.DeleteAccountAsync(request.Id);
                await _accountService.SaveAsync();

                _logger.LogInformation(
                    "TraderAccount deleted: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to delete TraderAccount: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}