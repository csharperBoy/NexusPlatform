using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Commands.TradingRun;

public record CreateTradingRunCommand(
    CreateTradingRunModel Model
) : IRequest<Result<Guid>>;

public class CreateTradingRunCommandHandler
    : IRequestHandler<CreateTradingRunCommand, Result<Guid>>
{
    private readonly ITradingRunCommandService _service;
    private readonly ILogger<CreateTradingRunCommandHandler> _logger;

    public CreateTradingRunCommandHandler(
        ITradingRunCommandService service,
        ILogger<CreateTradingRunCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateTradingRunCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating TradingRun {Name} strategy={Strategy}",
                request.Model.Name, request.Model.StrategyKey);

            var id = await _service.CreateAsync(request.Model);

            _logger.LogInformation("TradingRun created {Id}", id);
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create TradingRun {Name}", request.Model.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}