using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;

namespace Trader.Application.Commands.Backtest;

public record CancelBacktestCommand(Guid Id) : IRequest<Result<bool>>;

public class CancelBacktestCommandHandler
    : IRequestHandler<CancelBacktestCommand, Result<bool>>
{
    private readonly IBacktestRunCommandService _service;
    private readonly ILogger<CancelBacktestCommandHandler> _logger;

    public CancelBacktestCommandHandler(
        IBacktestRunCommandService service,
        ILogger<CancelBacktestCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelBacktestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _service.CancelAsync(request.Id, cancellationToken);
            if (!ok)
                return Result<bool>.Fail(
                    $"BacktestRun {request.Id} not found or not cancellable.");

            await _service.SaveAsync(cancellationToken);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel BacktestRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}