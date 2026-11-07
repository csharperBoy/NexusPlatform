using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class HistoricalDataCollectionService
    : IHistoricalDataCollectionCommandService,
      IHistoricalDataCollectionQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<HistoricalDataCollectionService> _logger;

    public HistoricalDataCollectionService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<HistoricalDataCollectionService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /* ═══════════════════ Commands ═══════════════════ */

    public async Task<Guid> CreateAsync(
        string name,
        List<string> isins,
        List<int> timeFrames,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default)
    {
        if (isins is null || isins.Count == 0)
            throw new InvalidOperationException("حداقل یک نماد الزامی است.");

        if (timeFrames is null || timeFrames.Count == 0)
            throw new InvalidOperationException("حداقل یک تایم‌فریم الزامی است.");

        if (dateTo <= dateFrom)
            throw new InvalidOperationException("تاریخ پایان باید بعد از تاریخ شروع باشد.");

        var distinctIsins = isins
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var distinctTfs = timeFrames.Distinct().ToList();

        if (distinctIsins.Count == 0)
            throw new InvalidOperationException("هیچ ISIN معتبری وجود ندارد.");

        var isinsJson = JsonSerializer.Serialize(distinctIsins);
        var tfsJson = JsonSerializer.Serialize(distinctTfs);

        var job = HistoricalDataCollectionJob.Create(
            name: name,
            isinsJson: isinsJson,
            timeFramesJson: tfsJson,
            dateFrom: dateFrom,
            dateTo: dateTo,
            totalCombinations: distinctIsins.Count * distinctTfs.Count);

        await _uow.HistoricalDataCollectionJobs.AddAsync(job);

        _logger.LogInformation(
            "Collection job created: {JobId} name={Name} isins={Isins} tfs={Tfs}",
            job.Id, job.Name, distinctIsins.Count, distinctTfs.Count);

        return job.Id;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _uow.HistoricalDataCollectionJobs.GetByIdAsync(id);
        if (job is null) return false;

        if (job.Status != CollectionJobStatus.Pending
            && job.Status != CollectionJobStatus.Running)
            return false;

        job.MarkCancelled();
        await _uow.HistoricalDataCollectionJobs.UpdateAsync(job);

        _logger.LogInformation("Collection job {JobId} cancelled.", id);
        return true;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _uow.SaveChangesAsync(ct);
    }

    /* ═══════════════════ Queries ═══════════════════ */

    public async Task<IReadOnlyList<HistoricalDataCollectionJobSummaryView>> GetListAsync(
    int take = 50, CancellationToken ct = default)
    {
        var takeClamped = Math.Clamp(take, 1, 500);

        // ─── SQL-side order + take ───
        var jobs = await _uow.HistoricalDataCollectionJobs.GetAllAsync(q =>
            q.OrderByDescending(j => j.CreatedAt).Take(takeClamped));

        return jobs.Select(MapToSummary).ToList();
    }

    public async Task<HistoricalDataCollectionJobDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var job = await _uow.HistoricalDataCollectionJobs.GetByIdAsync(id);
        return job is null ? null : MapToDetail(job);
    }

    /* ═══════════════════ Mappers ═══════════════════ */

    private static HistoricalDataCollectionJobSummaryView MapToSummary(
        HistoricalDataCollectionJob j) => new(
            j.Id, j.Name,
            (int)j.Status, j.Status.ToString(),
            j.TotalCombinations, j.CompletedCombinations, j.TotalCandlesCollected,
            j.ProgressPercent, j.CurrentIsin, j.CurrentTimeFrame,
            j.StartedAt, j.CompletedAt, j.DurationMs, j.ErrorMessage, j.CreatedAt);

    private static HistoricalDataCollectionJobDetailView MapToDetail(
        HistoricalDataCollectionJob j)
    {
        List<string> isins;
        List<int> timeFrames;

        try { isins = JsonSerializer.Deserialize<List<string>>(j.IsinsJson) ?? new(); }
        catch { isins = new(); }

        try { timeFrames = JsonSerializer.Deserialize<List<int>>(j.TimeFramesJson) ?? new(); }
        catch { timeFrames = new(); }

        return new HistoricalDataCollectionJobDetailView(
            j.Id, j.Name,
            (int)j.Status, j.Status.ToString(),
            isins, timeFrames,
            j.DateFrom, j.DateTo,
            j.TotalCombinations, j.CompletedCombinations, j.TotalCandlesCollected,
            j.ProgressPercent, j.CurrentIsin, j.CurrentTimeFrame,
            j.StartedAt, j.CompletedAt, j.DurationMs, j.ErrorMessage, j.CreatedAt);
    }
}