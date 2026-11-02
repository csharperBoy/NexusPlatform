using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Commands.Backtest;

public record RunBacktestCommand(RunBacktestModel Model) : IRequest<Result<Guid>>;

public class RunBacktestCommandHandler
    : IRequestHandler<RunBacktestCommand, Result<Guid>>
{
    private readonly IBacktestRunCommandService _service;
    private readonly ILogger<RunBacktestCommandHandler> _logger;

    public RunBacktestCommandHandler(
        IBacktestRunCommandService service,
        ILogger<RunBacktestCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        RunBacktestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _service.CreateAsync(request.Model, cancellationToken);
            await _service.SaveAsync(cancellationToken);

            _logger.LogInformation(
                "BacktestRun {Id} queued for execution.", id);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create BacktestRun {Name}", request.Model.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}