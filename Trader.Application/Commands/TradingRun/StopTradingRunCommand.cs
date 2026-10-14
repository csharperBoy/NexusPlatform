using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Domain.Enums;

namespace Trader.Application.Commands.TradingRun;

public record StopTradingRunCommand(
    Guid Id,
    int Behavior  // 0 = KeepPositions, 1 = ClosePositions
) : IRequest<Result<bool>>;

public class StopTradingRunCommandHandler
    : IRequestHandler<StopTradingRunCommand, Result<bool>>
{
    private readonly ITradingRunCommandService _service;
    private readonly ILogger<StopTradingRunCommandHandler> _logger;

    public StopTradingRunCommandHandler(
        ITradingRunCommandService service,
        ILogger<StopTradingRunCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StopTradingRunCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var behavior = (StopRunBehavior)request.Behavior;

            _logger.LogInformation(
                "Stopping TradingRun {Id} behavior={Behavior}",
                request.Id, behavior);

            await _service.StopAsync(request.Id, behavior);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop TradingRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}