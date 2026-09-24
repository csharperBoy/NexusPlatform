using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Core.Shared.Results;

namespace Trader.Application.Commands.Account
{
    public record CreateAccountCommand(
        string Name,
        string Username,
        string Password
    ) : IRequest<Result<Guid>>;

    public class CreateAccountCommandHandler
        : IRequestHandler<CreateAccountCommand, Result<Guid>>
    {
        private readonly IAccountCommandService _accountService;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(
            IAccountCommandService accountService,
            ILogger<CreateAccountCommandHandler> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            CreateAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating TraderAccount: {Name}", request.Name);

                var id = await _accountService.CreateAccountAsync(
                    request.Name,
                    request.Username,
                    request.Password);

                await _accountService.SaveAsync();

                _logger.LogInformation(
                    "TraderAccount created: {Id} ({Name})", id, request.Name);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create TraderAccount: {Name}", request.Name);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}