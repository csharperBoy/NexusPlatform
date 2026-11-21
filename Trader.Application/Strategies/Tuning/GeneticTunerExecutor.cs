// Trader.Application/Strategies/Tuning/GeneticTunerExecutor.cs
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Strategies.Tuning;

public sealed record GeneticTuningRequest(
    Guid TuningRunId,
    IStrategyTuner Tuner,
    string Isin,
    string? TargetCurrency,
    TimeFrame PrimaryTimeFrame,
    TimeFrame? SubCandleTimeFrame,
    DateTime DateFrom,
    DateTime DateTo,
    decimal InitialCapital,
    decimal SlippagePercent,
    TuningScale SelectedScales,
    TuningScale PrimaryScale,
    int MinTradesPerWindow,
    int PopulationSize,
    int Generations,
    decimal MutationRate,
    decimal CrossoverRate,
    int RequiredCandleCount,
    IReadOnlyList<ParameterDefinition> TunableParameters);

public sealed record TuningProgress(int CompletedEvals, int TotalEvals, int Generation);


public sealed record TuningResultData(
    System.Text.Json.Nodes.JsonObject Config,
    int Generation,
    ScaleMetrics? Yearly, ScaleMetrics? SemiAnnual,
    ScaleMetrics? Quarterly, ScaleMetrics? Monthly,
    string WindowResultsJson,
    TuningResultStatus Status,
    long RuntimeMs)
{
    public ScaleMetrics? GetScale(TuningScale s) => s switch
    {
        TuningScale.Yearly => Yearly,
        TuningScale.SemiAnnual => SemiAnnual,
        TuningScale.Quarterly => Quarterly,
        TuningScale.Monthly => Monthly,
        _ => null,
    };

    public decimal GetScore(TuningScale primary) =>
        GetScale(primary)?.AvgCompound ?? decimal.MinValue;
}
// ─── Interface ───
public sealed record GeneticTuningResult(
    IReadOnlyList<TuningResultData> AllEvaluations);

public interface IStrategyTunerExecutor
{
    Task<GeneticTuningResult> ExecuteAsync(
        GeneticTuningRequest request,
        IProgress<TuningProgress>? progress,
        CancellationToken ct = default);
}
public sealed class GeneticTunerExecutor : IStrategyTunerExecutor
{
    private readonly IBacktestSimulator _simulator;
    private readonly ILogger<GeneticTunerExecutor> _logger;

    public GeneticTunerExecutor(
        IBacktestSimulator simulator,
        ILogger<GeneticTunerExecutor> logger)
    {
        _simulator = simulator;
        _logger = logger;
    }

    public async Task<GeneticTuningResult> ExecuteAsync(
        GeneticTuningRequest req, IProgress<TuningProgress>? progress, CancellationToken ct)
    {
        var windows = TuningWindowBuilder.BuildAll(
            req.DateFrom, req.DateTo, req.SelectedScales, req.TuningRunId);

        var space = req.TunableParameters;
        var defaults = req.Tuner.GetDefaults(req.PrimaryTimeFrame);

        // ─── Initialize population ───
        var population = new List<JsonObject>(req.PopulationSize);
        population.Add(defaults);  // elite: defaults
        for (int i = 1; i < req.PopulationSize; i++)
            population.Add(RandomChromosome(space, req.Tuner, req.PrimaryTimeFrame));

        var bestPerScale = new Dictionary<TuningScale, (JsonObject cfg, int gen, decimal score, Dictionary<TuningScale, ScaleMetrics> metrics)>();
        int totalEvals = 0;
        var allEvaluations = new List<TuningResultData>(req.PopulationSize * req.Generations);

        for (int gen = 0; gen < req.Generations; gen++)
        {
            ct.ThrowIfCancellationRequested();

            var evalTasks = population.Select((cfg, idx) =>
                Task.Run(() => EvaluateAsync(cfg, gen, windows, req, ct), ct));
            var results = await Task.WhenAll(evalTasks);
            allEvaluations.AddRange(results);

            totalEvals += req.PopulationSize;
            progress?.Report(new TuningProgress(totalEvals,
                req.PopulationSize * req.Generations, gen + 1));

            if (gen < req.Generations - 1)
                population = Evolve(population, results, req, space);
        }

        return new GeneticTuningResult(allEvaluations);
    }

    // ═══════════════════════════════════════════
    // Evaluation
    // ═══════════════════════════════════════════

    private async Task<TuningResultData> EvaluateAsync(
    JsonObject config, int gen,
    IReadOnlyDictionary<TuningScale, IReadOnlyList<TuningWindow>> windows,
    GeneticTuningRequest req, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var validated = req.Tuner.RepairConstraints(config, req.PrimaryTimeFrame);
        var error = req.Tuner.ValidateConfiguration(validated, req.PrimaryTimeFrame);
        if (error != null)
            return new TuningResultData(validated, gen, null, null, null, null,
                "{}", TuningResultStatus.Failed, sw.ElapsedMilliseconds);

        var configJson = validated.ToJsonString();
        var perScale = new Dictionary<TuningScale, ScaleMetrics>();
        var perScaleWindows = new Dictionary<TuningScale, List<object>>();

        foreach (var (scale, scaleWindows) in windows)
        {
            var returns = new List<decimal>();
            var windowDetails = new List<object>();
            int totalTrades = 0;

            foreach (var w in scaleWindows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sim = await _simulator.SimulateAsync(new BacktestSimulationRequest(
                        StrategyKey: req.Tuner.TunerKey,
                        StrategyConfigJson: configJson,
                        PrimaryTimeFrame: req.PrimaryTimeFrame,
                        SubCandleTimeFrame: req.SubCandleTimeFrame,
                        TargetCurrency: req.TargetCurrency,
                        DateFrom: w.From, DateTo: w.To,
                        InitialCapital: req.InitialCapital,
                        SlippagePercent: req.SlippagePercent,
                        StrengthMultiplier: 1.5m,
                        MinDeltaPercent: 0.03m,
                        StopLossPercentage: null, StopLossPrice: null,
                        TakeProfitPercentage: null, TakeProfitPrice: null,
                        Symbols: new[] { new BacktestSimulationSymbol(req.Isin, 100m) }),
                        NullBacktestEventLogger.Instance, ct);

                    var trades = sim.Metrics.TotalTrades;

                    // Filter windows with too few trades if requested
                    if (req.MinTradesPerWindow > 0 && trades < req.MinTradesPerWindow)
                        continue;

                    returns.Add(sim.Metrics.TotalReturnPct);
                    windowDetails.Add(new
                    {
                        from = w.From.ToString("yyyy-MM-dd"),
                        to = w.To.ToString("yyyy-MM-dd"),
                        ret = Math.Round(sim.Metrics.TotalReturnPct, 4),
                        trades,
                    });
                    totalTrades += trades;
                }
                catch { /* window failed — excluded */ }
            }

            if (returns.Count == 0)
            {
                perScale[scale] = new ScaleMetrics(0m, 0m, 0, totalTrades);
                perScaleWindows[scale] = windowDetails;
                continue;
            }

            var avgArith = Math.Round(returns.Average(), 4);
            var avgComp = ComputeCompound(returns);
            perScale[scale] = new ScaleMetrics(avgArith, avgComp, returns.Count, totalTrades);
            perScaleWindows[scale] = windowDetails;
        }

        sw.Stop();

        var status = perScale.Values.Any(m => m.ValidWindows > 0)
            ? TuningResultStatus.Success
            : TuningResultStatus.InsufficientData;

        var windowJson = JsonSerializer.Serialize(perScaleWindows.ToDictionary(
            kvp => kvp.Key.ToString(), kvp => kvp.Value));

        return new TuningResultData(
            Config: validated,
            Generation: gen,
            Yearly: perScale.TryGetValue(TuningScale.Yearly, out var y) ? y : null,
            SemiAnnual: perScale.TryGetValue(TuningScale.SemiAnnual, out var s) ? s : null,
            Quarterly: perScale.TryGetValue(TuningScale.Quarterly, out var q) ? q : null,
            Monthly: perScale.TryGetValue(TuningScale.Monthly, out var m) ? m : null,
            WindowResultsJson: windowJson,
            Status: status,
            RuntimeMs: sw.ElapsedMilliseconds);
    }

    private static decimal ComputeCompound(List<decimal> returnsPct)
    {
        if (returnsPct.Count == 0) return 0m;
        decimal product = 1m;
        foreach (var r in returnsPct) product *= (1m + r / 100m);
        if (product <= 0m) return -100m;
        var avg = Math.Pow((double)product, 1.0 / returnsPct.Count) - 1.0;
        return (decimal)(avg * 100.0);
    }

    // ═══════════════════════════════════════════
    // GA Operators
    // ═══════════════════════════════════════════

    private static JsonObject RandomChromosome(
        IReadOnlyList<ParameterDefinition> space,
        IStrategyTuner tuner, TimeFrame primaryTf)
    {
        var obj = new JsonObject();
        var rnd = Random.Shared;
        foreach (var p in space)
        {
            switch (p)
            {
                case IntParameter ip:
                    obj[ip.Name] = rnd.Next(ip.Min, ip.Max + 1);
                    break;
                case DecimalParameter dp:
                    var range = dp.Max - dp.Min;
                    var v = dp.Min + (decimal)rnd.NextDouble() * range;
                    obj[dp.Name] = Math.Round(v, 4);
                    break;
                case BoolParameter bp:
                    obj[bp.Name] = rnd.Next(2) == 0;
                    break;
                case TimeFrameParameter tfp:
                    obj[tfp.Name] = tfp.AllowedValues[rnd.Next(tfp.AllowedValues.Count)];
                    break;
            }
        }
        // add missing from defaults (e.g., MicroWeight which is derived)
        var defaults = tuner.GetDefaults(primaryTf);
        foreach (var kvp in defaults)
            if (!obj.ContainsKey(kvp.Key)) obj[kvp.Key] = kvp.Value?.DeepClone();
        // recompute derived weights
        var tW = obj["TrendWeight"]?.GetValue<decimal>() ?? 0.4m;
        var mW = obj["MomentumWeight"]?.GetValue<decimal>() ?? 0.35m;
        obj["MicroWeight"] = 1m - tW - mW;
        return tuner.RepairConstraints(obj, primaryTf);
    }

    private List<JsonObject> Evolve(
    List<JsonObject> pop, TuningResultData[] results,
    GeneticTuningRequest req, IReadOnlyList<ParameterDefinition> space)
    {
        var ranked = pop.Zip(results)
            .OrderByDescending(x => x.Second.GetScore(req.PrimaryScale))
            .ToList();

        var next = new List<JsonObject>(req.PopulationSize);
        next.Add(ranked[0].First.DeepClone().AsObject());
        next.Add(ranked[1].First.DeepClone().AsObject());

        while (next.Count < req.PopulationSize)
        {
            var p1 = TournamentSelect(ranked, req.PrimaryScale);
            var p2 = TournamentSelect(ranked, req.PrimaryScale);
            var child = (Random.Shared.NextDouble() < (double)req.CrossoverRate)
                ? Crossover(p1, p2, space)
                : p1.DeepClone().AsObject();

            Mutate(child, space, req.MutationRate);
            next.Add(req.Tuner.RepairConstraints(child, req.PrimaryTimeFrame));
        }
        return next;
    }

    private static JsonObject TournamentSelect(
        List<(JsonObject cfg, TuningResultData res)> ranked, TuningScale primary)
    {
        var cands = new List<(JsonObject cfg, TuningResultData res)>();
        for (int i = 0; i < 5; i++)
            cands.Add(ranked[Random.Shared.Next(ranked.Count)]);
        return cands.OrderByDescending(c => c.res.GetScore(primary)).First().cfg;
    }

   

    private static JsonObject Crossover(
        JsonObject a, JsonObject b, IReadOnlyList<ParameterDefinition> space)
    {
        var child = new JsonObject();
        foreach (var p in space)
        {
            var src = Random.Shared.Next(2) == 0 ? a : b;
            child[p.Name] = src[p.Name]?.DeepClone();
        }
        return child;
    }

    private static void Mutate(JsonObject obj, IReadOnlyList<ParameterDefinition> space, decimal rate)
    {
        var rnd = Random.Shared;
        foreach (var p in space)
        {
            if (rnd.NextDouble() >= (double)rate) continue;
            switch (p)
            {
                case IntParameter ip:
                    obj[ip.Name] = rnd.Next(ip.Min, ip.Max + 1); break;
                case DecimalParameter dp:
                    var v = dp.Min + (decimal)rnd.NextDouble() * (dp.Max - dp.Min);
                    obj[dp.Name] = Math.Round(v, 4); break;
                case BoolParameter bp:
                    obj[bp.Name] = rnd.Next(2) == 0; break;
                case TimeFrameParameter tfp:
                    obj[tfp.Name] = tfp.AllowedValues[rnd.Next(tfp.AllowedValues.Count)]; break;
            }
        }
    }

    private static string BuildWindowResultsJson(Dictionary<TuningScale, ScaleMetrics> metrics)
    {
        // summary only; full per-window results would explode storage
        var summary = metrics.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => new { compound = kvp.Value.AvgCompound, arithmetic = kvp.Value.AvgArithmetic, trades = kvp.Value.TotalTrades });
        return JsonSerializer.Serialize(summary);
    }

    private sealed record EvalResult(
        JsonObject Config, int Generation,
        Dictionary<TuningScale, ScaleMetrics> MetricsByScale,
        string? Error);
}