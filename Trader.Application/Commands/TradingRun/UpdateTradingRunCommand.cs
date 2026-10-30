using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Commands.TradingRun;

public record UpdateTradingRunCommand(
    Guid Id,
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<Guid> AccountIds
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
            var model = new UpdateTradingRunModel(
                request.Name,
                request.StrategyKey,
                request.StrategyConfigJson,
                request.PrimaryTimeFrame,
    request.TargetCurrency,
                request.StrengthMultiplier,
    request.MinDeltaPercent,
                request.Settings,
                request.Symbols,
                request.AccountIds);

            await _service.UpdateAsync(request.Id, model);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update TradingRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}