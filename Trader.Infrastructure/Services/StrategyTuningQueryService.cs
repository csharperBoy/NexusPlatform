// Trader.Infrastructure/Services/StrategyTuningQueryService.cs
using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Tuning;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class StrategyTuningQueryService : IStrategyTuningQueryService
{
    private readonly TraderDbContext _db;
    public StrategyTuningQueryService(TraderDbContext db) { _db = db; }

    public async Task<IReadOnlyList<StrategyTuningRunSummaryView>> GetListAsync(
        int take, CancellationToken ct)
    {
        return await _db.StrategyTuningRuns.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).Take(take)
            .Select(x => new StrategyTuningRunSummaryView(
                x.Id, x.Name, x.StrategyKey, x.Isin,
                (int)x.PrimaryTimeFrame, x.PrimaryTimeFrame.ToString(),
                x.DateFrom, x.DateTo,
                (int)x.Status, x.Status.ToString(),
                x.TotalEvaluations, x.CompletedEvaluations,
                x.StartedAt, x.CompletedAt, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<StrategyTuningRunDetailView?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var run = await _db.StrategyTuningRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (run is null) return null;

        var topResults = await _db.StrategyTuningResults.AsNoTracking()
            .Where(r => r.TuningRunId == id &&
                (r.IsTop10InYearly || r.IsTop10InSemi || r.IsTop10InQuarterly || r.IsTop10InMonthly))
            .OrderByDescending(r => r.AvgReturnCompoundYearly)
            .Select(r => Map(r))
            .ToListAsync(ct);

        return new StrategyTuningRunDetailView(
            run.Id, run.Name, run.StrategyKey, run.Isin, run.TargetCurrency,
            (int)run.PrimaryTimeFrame,
            run.SubCandleTimeFrame.HasValue ? (int)run.SubCandleTimeFrame.Value : null,
            run.DateFrom, run.DateTo, run.InitialCapital, run.SlippagePercent,
            (int)run.SelectedScales, (int)run.PrimaryScale, run.MinTradesPerWindow,
            run.PopulationSize, run.Generations, run.MutationRate, run.CrossoverRate,
            (int)run.Status, run.Status.ToString(),
            run.TotalEvaluations, run.CompletedEvaluations,
            run.BestResultIdYearly, run.BestResultIdSemi,
            run.BestResultIdQuarterly, run.BestResultIdMonthly,
            run.ErrorMessage,
            run.StartedAt, run.CompletedAt, run.CreatedAt, topResults);
    }

    public async Task<IReadOnlyList<StrategyTuningResultView>> GetResultsAsync(
        Guid tuningRunId, bool topOnly, int take, CancellationToken ct)
    {
        var q = _db.StrategyTuningResults.AsNoTracking()
            .Where(r => r.TuningRunId == tuningRunId);
        if (topOnly) q = q.Where(r =>
            r.IsTop10InYearly || r.IsTop10InSemi || r.IsTop10InQuarterly || r.IsTop10InMonthly);

        return await q.OrderByDescending(r => r.AvgReturnCompoundYearly)
            .Take(take).Select(r => Map(r)).ToListAsync(ct);
    }

    private static StrategyTuningResultView Map(Domain.Entities.StrategyTuningResult r) =>
        new(r.Id, r.Generation, (int)r.Status, r.ConfigJson,
            r.AvgReturnCompoundYearly, r.AvgReturnCompoundSemi,
            r.AvgReturnCompoundQuarter, r.AvgReturnCompoundMonthly,
            r.ValidWindowsYearly, r.ValidWindowsSemi,
            r.ValidWindowsQuarter, r.ValidWindowsMonthly,
            r.TotalTradesYearly, r.TotalTradesSemi,
            r.TotalTradesQuarter, r.TotalTradesMonthly,
            r.IsTop10InYearly, r.IsTop10InSemi,
            r.IsTop10InQuarterly, r.IsTop10InMonthly, r.RuntimeMs);
}