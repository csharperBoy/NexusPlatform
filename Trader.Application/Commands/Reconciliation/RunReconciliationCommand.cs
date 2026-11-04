using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Reconciliation;
using Trader.Application.Dtos.Reconciliation;

namespace Trader.Application.Commands.Reconciliation;

public record RunReconciliationCommand : IRequest<Result<ReconciliationResult>>;

public class RunReconciliationCommandHandler
    : IRequestHandler<RunReconciliationCommand, Result<ReconciliationResult>>
{
    private readonly IReconciliationService _service;
    private readonly ILogger<RunReconciliationCommandHandler> _logger;

    public RunReconciliationCommandHandler(
        IReconciliationService service,
        ILogger<RunReconciliationCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<ReconciliationResult>> Handle(
        RunReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Manual reconciliation triggered.");

            var result = await _service.ReconcileAllAsync(cancellationToken);
            return Result<ReconciliationResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual reconciliation failed.");
            return Result<ReconciliationResult>.Fail(ex.Message);
        }
    }
}