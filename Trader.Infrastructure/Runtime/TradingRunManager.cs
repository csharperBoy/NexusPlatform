using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Application.Specifications.TradingRuns;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Runtime;

public sealed class TradingRunManager : ITradingRunManager
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IRunExecutionContextCache _cache;
    private readonly IStrategyFactory _strategyFactory;
    private readonly ILogger<TradingRunManager> _logger;

    public TradingRunManager(
        ITraderUnitOfWork<TraderDbContext> uow,
        IRunExecutionContextCache cache,
        IStrategyFactory strategyFactory,
        ILogger<TradingRunManager> logger)
    {
        _uow = uow;
        _cache = cache;
        _strategyFactory = strategyFactory;
        _logger = logger;
    }
    public async Task StartAsync(Guid runId, CancellationToken ct = default)
    {
        if (_cache.Contains(runId))
            _logger.LogInformation(
                "Run {RunId} already active; refreshing context.", runId);

        var spec = new TradingRunByIdWithChildrenSpec(runId);
        var run = await _uow.TradingRunSpecs.GetBySpecAsync(spec)
            ?? throw new InvalidOperationException($"TradingRun {runId} not found.");

        if (run.State != TradingRunState.Running)
            throw new InvalidOperationException(
                $"Cannot activate run {runId}: state is {run.State}.");

        if (run.OwnerUserId is null || run.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException(
                $"TradingRun {runId} has no OwnerUserId.");

        var config = _strategyFactory.DeserializeConfig(
            run.StrategyKey, run.StrategyConfigJson);
        var strategy = _strategyFactory.Create(run.StrategyKey, config);

        var symbols = run.Symbols
            .Select(s => new RunSymbolContext(s.Isin, s.Allocation))
            .ToList();

        var accountIds = run.Accounts
            .Select(a => a.TraderAccountId)
            .ToList();

        var context = new RunRuntimeContext(
            RunId: run.Id,
            Name: run.Name,
            OwnerPersonId: run.OwnerPersonId ?? Guid.Empty,
            OwnerUserId: run.OwnerUserId.Value,
            StrategyKey: run.StrategyKey,
            Strategy: strategy,
            StrategyConfig: config,
            PrimaryTimeFrame: run.PrimaryTimeFrame,
            CapitalAllocation: run.CapitalAllocation,
            StopLoss: run.StopLoss,
            TakeProfit: run.TakeProfit,
            DefaultOrderMode: run.DefaultOrderMode,
            OnMarketClose: run.OnMarketClose,
            OnStop: run.OnStop,
            Symbols: symbols,
            AccountIds: accountIds);

        _cache.Set(context);

        _logger.LogInformation(
            "TradingRun activated: {RunId} owner={Owner} strategy={Strategy}",
            runId, run.OwnerUserId, run.StrategyKey);
    }

    public Task StopAsync(Guid runId, StopRunBehavior behavior, CancellationToken ct = default)
    {
        if (!_cache.Contains(runId))
        {
            _logger.LogInformation(
                "Stop requested for inactive run {RunId}; no-op.", runId);
            return Task.CompletedTask;
        }

        _cache.Remove(runId);

        _logger.LogInformation(
            "TradingRun deactivated: {RunId} behavior={Behavior}",
            runId, behavior);

        // ClosePositions: در فاز بعد که ExecutionCoordinator پیاده شد،
        // اینجا صدا زده می‌شود.
        if (behavior == StopRunBehavior.ClosePositions)
        {
            _logger.LogWarning(
                "ClosePositions requested for run {RunId}, but not yet implemented.",
                runId);
        }

        return Task.CompletedTask;
    }

    public bool IsRunning(Guid runId) => _cache.Contains(runId);
}