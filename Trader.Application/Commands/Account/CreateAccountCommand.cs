using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Domain.Enums;

namespace Trader.Application.Commands.Account
{
    public record CreateAccountCommand(
        int Broker,
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
                var broker = (BrokerType)request.Broker;

                _logger.LogInformation(
                    "Creating TraderAccount: {Name} broker={Broker}",
                    request.Name, broker);

                var id = await _accountService.CreateAccountAsync(
                    broker,
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