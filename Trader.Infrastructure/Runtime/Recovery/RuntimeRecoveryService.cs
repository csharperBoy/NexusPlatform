using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Specifications.TradingRuns;

namespace Trader.Infrastructure.Runtime.Recovery;

public sealed class RuntimeRecoveryService : IRuntimeRecoveryService
{
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly ITradingRunManager _runManager;
    private readonly ILogger<RuntimeRecoveryService> _logger;

    public RuntimeRecoveryService(
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        ITradingRunManager runManager,
        ILogger<RuntimeRecoveryService> logger)
    {
        _uow = uow;
        _runManager = runManager;
        _logger = logger;
    }

    public async Task RecoverAsync(CancellationToken ct = default)
    {
        var spec = new RunningTradingRunsSpec();
        var runs = await _uow.TradingRunSpecs.ListBySpecAsync(spec);

        if (runs.Count == 0)
        {
            _logger.LogInformation("Runtime recovery: no running runs to restore.");
            return;
        }

        _logger.LogInformation(
            "Runtime recovery: restoring {Count} running runs.", runs.Count);

        foreach (var run in runs)
        {
            try
            {
                await _runManager.StartAsync(run.Id, ct);
                _logger.LogInformation("Restored run {RunId} ({Name})",
                    run.Id, run.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to restore run {RunId} ({Name}). Marking faulted.",
                    run.Id, run.Name);

                try
                {
                    run.MarkFaulted($"Recovery failed: {ex.Message}");
                    await _uow.TradingRuns.UpdateAsync(run);
                    await _uow.SaveChangesAsync(ct);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx,
                        "Failed to mark run {RunId} as faulted.", run.Id);
                }
            }
        }
    }
}