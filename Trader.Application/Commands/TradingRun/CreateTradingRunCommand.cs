using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Commands.TradingRun;

public record CreateTradingRunCommand(
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<Guid> AccountIds
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
            var model = new CreateTradingRunModel(
                request.Name,
                request.StrategyKey,
                request.StrategyConfigJson,
                request.PrimaryTimeFrame,
                request.Settings,
                request.Symbols,
                request.AccountIds);

            var id = await _service.CreateAsync(model);

            _logger.LogInformation(
                "TradingRun created {Id} name={Name} strategy={Strategy}",
                id, request.Name, request.StrategyKey);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create TradingRun {Name}", request.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}