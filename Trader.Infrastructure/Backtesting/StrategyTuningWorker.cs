using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Health;
using Trader.Application.Specifications.Tuning;
using Trader.Application.Strategies.Tuning;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

/// <summary>
/// Polls StrategyTuningRuns in Pending state, runs them via IStrategyTunerExecutor,
/// persists all evaluations, marks Top-10 per scale, and updates the run's status.
///
/// Only one tuning run executes at a time (heavy compute). Parallel within
/// the GA (evaluation of population) is handled inside GeneticTunerExecutor.
///
/// Status transitions:
///   Pending → Running → Completed | Failed | Cancelled
/// </summary>
public sealed class StrategyTuningWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRuntimeHealthState _healthState;
    private readonly ILogger<StrategyTuningWorker> _logger;

    public StrategyTuningWorker(
        IServiceScopeFactory scopeFactory,
        IRuntimeHealthState healthState,
        ILogger<StrategyTuningWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _healthState = healthState;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StrategyTuningWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StrategyTuningWorker iteration failed.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("StrategyTuningWorker stopped.");
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        _healthState.MarkBacktestPollNow(); // reuse existing health signal

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var executor = scope.ServiceProvider.GetRequiredService<IStrategyTunerExecutor>();

        var tuners = scope.ServiceProvider.GetServices<IStrategyTuner>()
            .ToDictionary(t => t.TunerKey);

        if (tuners.Count == 0)
        {
            _logger.LogWarning("No IStrategyTuner registered — tuning cannot run.");
            return;
        }

        var pending = (await uow.StrategyTuningRunSpecs.ListBySpecAsync(
            new PendingStrategyTuningRunsSpec())).ToList();

        if (pending.Count == 0) return;

        _logger.LogInformation("Found {Count} pending tuning runs.", pending.Count);

        foreach (var run in pending)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await ExecuteRunAsync(run, uow, executor, tuners, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "StrategyTuningRun {Id} threw unhandled exception.", run.Id);
            }
        }
    }

    /* ═══════════════════ Per-Run Execution ═══════════════════ */

    private async Task ExecuteRunAsync(
        StrategyTuningRun run,
        ITraderUnitOfWork<TraderDbContext> uow,
        IStrategyTunerExecutor executor,
        IReadOnlyDictionary<string, IStrategyTuner> tuners,
        CancellationToken ct)
    {
        if (!tuners.TryGetValue(run.StrategyKey, out var tuner))
        {
            run.MarkFailed($"No tuner registered for strategy '{run.StrategyKey}'.");
            await uow.StrategyTuningRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(ct);
            _logger.LogWarning("TuningRun {Id}: no tuner for strategy {Key}.",
                run.Id, run.StrategyKey);
            return;
        }

        run.MarkRunning();
        await uow.StrategyTuningRuns.UpdateAsync(run);
        await uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "TuningRun {Id} started: strategy={Strategy} isin={Isin} " +
            "range={From:yyyy-MM-dd}→{To:yyyy-MM-dd} scales={Scales} pop={Pop} gens={Gens}",
            run.Id, run.StrategyKey, run.Isin, run.DateFrom, run.DateTo,
            run.SelectedScales, run.PopulationSize, run.Generations);

        try
        {
            var request = new GeneticTuningRequest(
                TuningRunId: run.Id,
                Tuner: tuner,
                Isin: run.Isin,
                TargetCurrency: run.TargetCurrency,
                PrimaryTimeFrame: run.PrimaryTimeFrame,
                SubCandleTimeFrame: run.SubCandleTimeFrame,
                DateFrom: run.DateFrom,
                DateTo: run.DateTo,
                InitialCapital: run.InitialCapital,
                SlippagePercent: run.SlippagePercent,
                SelectedScales: run.SelectedScales,
                PrimaryScale: run.PrimaryScale,
                MinTradesPerWindow: run.MinTradesPerWindow,
                PopulationSize: run.PopulationSize,
                Generations: run.Generations,
                MutationRate: run.MutationRate,
                CrossoverRate: run.CrossoverRate,
                RequiredCandleCount: 200);

            // Progress reporter — fire-and-forget per generation
            var runIdCapture = run.Id;
            var progress = new Progress<TuningProgress>(p =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var s = _scopeFactory.CreateScope();
                        var u = s.ServiceProvider
                            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
                        var r = await u.StrategyTuningRuns.GetByIdAsync(runIdCapture);
                        if (r != null && r.Status == TuningStatus.Running)
                        {
                            r.ReportProgress(p.CompletedEvals);
                            await u.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex,
                            "Progress update failed for run {RunId}.", runIdCapture);
                    }
                });
            });

            var result = await executor.ExecuteAsync(request, progress, ct);

            _logger.LogInformation(
                "TuningRun {Id}: GA finished, {Count} evaluations collected.",
                run.Id, result.AllEvaluations.Count);

            // ─── Compute Top-10 per scale ───
            var top10Yearly = TopIndices(result.AllEvaluations, r => r.Yearly?.AvgCompound, 10);
            var top10Semi = TopIndices(result.AllEvaluations, r => r.SemiAnnual?.AvgCompound, 10);
            var top10Quarterly = TopIndices(result.AllEvaluations, r => r.Quarterly?.AvgCompound, 10);
            var top10Monthly = TopIndices(result.AllEvaluations, r => r.Monthly?.AvgCompound, 10);

            var topSet = new HashSet<int>();
            foreach (var i in top10Yearly) topSet.Add(i);
            foreach (var i in top10Semi) topSet.Add(i);
            foreach (var i in top10Quarterly) topSet.Add(i);
            foreach (var i in top10Monthly) topSet.Add(i);

            // ─── Persist all evaluations ───
            var entities = new List<StrategyTuningResult>(result.AllEvaluations.Count);
            for (int i = 0; i < result.AllEvaluations.Count; i++)
            {
                var data = result.AllEvaluations[i];
                var isTop = topSet.Contains(i);

                var entity = StrategyTuningResult.Create(
                    tuningRunId: run.Id,
                    configJson: data.Config.ToJsonString(),
                    generation: data.Generation,
                    status: data.Status,
                    runtimeMs: data.RuntimeMs,
                    yearly: data.Yearly,
                    semi: data.SemiAnnual,
                    quarterly: data.Quarterly,
                    monthly: data.Monthly,
                    windowResultsJson: isTop ? data.WindowResultsJson : null,
                    errorMessage: null);

                if (top10Yearly.Contains(i)) entity.MarkTop10(TuningScale.Yearly);
                if (top10Semi.Contains(i)) entity.MarkTop10(TuningScale.SemiAnnual);
                if (top10Quarterly.Contains(i)) entity.MarkTop10(TuningScale.Quarterly);
                if (top10Monthly.Contains(i)) entity.MarkTop10(TuningScale.Monthly);

                await uow.StrategyTuningResults.AddAsync(entity);
                entities.Add(entity);
            }

            await uow.SaveChangesAsync(ct);

            // ─── Mark completed with best-of-each-scale ───
            Guid? bestYearly = top10Yearly.Count > 0 ? entities[top10Yearly[0]].Id : null;
            Guid? bestSemi = top10Semi.Count > 0 ? entities[top10Semi[0]].Id : null;
            Guid? bestQuarterly = top10Quarterly.Count > 0 ? entities[top10Quarterly[0]].Id : null;
            Guid? bestMonthly = top10Monthly.Count > 0 ? entities[top10Monthly[0]].Id : null;

            run.MarkCompleted(bestYearly, bestSemi, bestQuarterly, bestMonthly);
            await uow.StrategyTuningRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "TuningRun {Id} completed. Results saved: {Total}, Top-10 union: {Top}. " +
                "Best: Yearly={BestY} Semi={BestS} Quarterly={BestQ} Monthly={BestM}",
                run.Id, result.AllEvaluations.Count, topSet.Count,
                bestYearly, bestSemi, bestQuarterly, bestMonthly);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("TuningRun {Id} cancelled by user.", run.Id);
            run.MarkCancelled();
            await uow.StrategyTuningRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TuningRun {Id} failed.", run.Id);
            run.MarkFailed(ex.Message.Length > 1900
                ? ex.Message[..1900]
                : ex.Message);
            await uow.StrategyTuningRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(CancellationToken.None);
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static List<int> TopIndices(
        IReadOnlyList<TuningResultData> all,
        Func<TuningResultData, decimal?> selector,
        int n)
    {
        return Enumerable.Range(0, all.Count)
            .Where(i => selector(all[i]).HasValue)
            .OrderByDescending(i => selector(all[i])!.Value)
            .Take(n)
            .ToList();
    }
}