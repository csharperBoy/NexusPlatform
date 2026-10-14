using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Commands.TradingRun;

public record UpdateTradingRunCommand(
    Guid Id,
    UpdateTradingRunModel Model
) : IRequest<Result<bool>>;

public class UpdateTradingRunCommandHandler
    : IRequestHandler<UpdateTradingRunCommand, Result<bool>>
{
    private readonly ITradingRunCommandService _service;
    private readonly ILogger<UpdateTradingRunCommandHandler> _logger;

    public UpdateTradingRunCommandHandler(
        ITradingRunCommandService service,
        ILogger<UpdateTradingRunCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateTradingRunCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(request.Id, request.Model);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update TradingRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}