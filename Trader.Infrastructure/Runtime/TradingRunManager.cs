using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Positions;
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
    private readonly IPositionCloser _positionCloser;
    private readonly ITradingEventLogger _eventLogger;
    public TradingRunManager(
        ITraderUnitOfWork<TraderDbContext> uow,
        IRunExecutionContextCache cache,
        IStrategyFactory strategyFactory, 
        IPositionCloser positionCloser,
         ITradingEventLogger eventLogger,
        ILogger<TradingRunManager> logger)
    {
        _uow = uow;
        _cache = cache;
        _strategyFactory = strategyFactory;
        _positionCloser = positionCloser;
        _eventLogger = eventLogger;
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
                        TargetCurrency: run.TargetCurrency,
                        StrengthMultiplier: run.StrengthMultiplier,
                        CapitalAllocation: run.CapitalAllocation,
                        StopLoss: run.StopLoss,
                        TakeProfit: run.TakeProfit,
                        DefaultOrderMode: run.DefaultOrderMode,
                        OnMarketClose: run.OnMarketClose,
                        OnStop: run.OnStop,
                        Symbols: symbols,
                        AccountIds: accountIds);

        _cache.Set(context);

        // ─── Lifecycle Event ───
        _eventLogger.Log(
            stage: TradingStage.Lifecycle,
            type: TradingEventType.RunStarted,
            severity: TradingEventSeverity.Info,
            message: $"پلن «{run.Name}» فعال شد. " +
                     $"استراتژی: {run.StrategyKey}, " +
                     $"تایم‌فریم: {run.PrimaryTimeFrame.ToCode()}, " +
                     $"نمادها: {symbols.Count}, " +
                     $"حساب‌ها: {accountIds.Count}" +
                     (string.IsNullOrEmpty(run.TargetCurrency)
                        ? ""
                        : $", ارز تحلیل: {run.TargetCurrency}"),
            payload: new
            {
                strategyKey = run.StrategyKey,
                timeFrame = run.PrimaryTimeFrame.ToCode(),
                symbolCount = symbols.Count,
                accountCount = accountIds.Count,
                targetCurrency = run.TargetCurrency,
            },
            runId: runId);

        _logger.LogInformation(
            "TradingRun activated: {RunId} owner={Owner} strategy={Strategy}",
            runId, run.OwnerUserId, run.StrategyKey);
    }

    public async Task StopAsync(Guid runId, StopRunBehavior behavior, CancellationToken ct = default)
    {
        if (!_cache.Contains(runId))
        {
            _logger.LogInformation(
                "Stop requested for inactive run {RunId}; no-op.", runId);
            return;
        }

        // نام پلن را قبل از Remove بخوانیم — از Cache
        var ctx = _cache.Get(runId);
        var runName = ctx?.Name ?? runId.ToString().Substring(0, 8);

        // ─── ClosePositions قبل از Remove از Cache ───

        if (behavior == StopRunBehavior.ClosePositions)
        {
            try
            {
                await _positionCloser.CloseRunPositionsAsync(
                    runId, "Run stopped with ClosePositions", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close positions for run {RunId}.", runId);
            }
        }

        _cache.Remove(runId);

        // ─── Lifecycle Event ───
        _eventLogger.Log(
            stage: TradingStage.Lifecycle,
            type: TradingEventType.RunStopped,
            severity: TradingEventSeverity.Info,
            message: $"پلن «{runName}» متوقف شد. " +
                     $"رفتار: {(behavior == StopRunBehavior.KeepPositions ? "حفظ موقعیت‌ها" : "بستن موقعیت‌ها")}",
            payload: new { behavior = behavior.ToString() },
            runId: runId);

        _logger.LogInformation(
            "TradingRun deactivated: {RunId} behavior={Behavior}",
            runId, behavior);
    }

    public bool IsRunning(Guid runId) => _cache.Contains(runId);
}