using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.TradingRun;

public record StartTradingRunCommand(Guid Id) : IRequest<Result<bool>>;

public class StartTradingRunCommandHandler
    : IRequestHandler<StartTradingRunCommand, Result<bool>>
{
    private readonly ITradingRunCommandService _service;
    private readonly ILogger<StartTradingRunCommandHandler> _logger;

    public StartTradingRunCommandHandler(
        ITradingRunCommandService service,
        ILogger<StartTradingRunCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StartTradingRunCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting TradingRun {Id}", request.Id);
            await _service.StartAsync(request.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start TradingRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}