using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Scheduler;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.HistoricalData;

public sealed class CollectCandleJobHandler : IScheduledJobHandler<CollectCandlePayload>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollectCandleJobHandler> _logger;

    public CollectCandleJobHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<CollectCandleJobHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(CollectCandlePayload payload, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var uow = sp.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(payload.JobId);

        if (job is null)
        {
            _logger.LogWarning(
                "CollectCandleJobHandler: job {Id} not found; skip.", payload.JobId);
            return;
        }

        if (job.State != HistoricalJobState.Running)
        {
            _logger.LogDebug(
                "CollectCandleJobHandler: job {Id} state={State}; skip.",
                payload.JobId, job.State);
            return;
        }

        var collector = sp.GetRequiredService<IHistoricalDataCollector>();

        try
        {
            if (job.Mode == HistoricalCollectionMode.Backfill)
                await collector.CollectBackfillAsync(job.Id, ct);
            else
                await collector.CollectRecurringAsync(job.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CollectCandleJobHandler failed: job={Id} mode={Mode}",
                job.Id, job.Mode);

            var fresh = await uow.HistoricalDataCollectionJobs.GetByIdAsync(job.Id);
            if (fresh is not null
                && fresh.State == HistoricalJobState.Running
                && fresh.Mode == HistoricalCollectionMode.Backfill)
            {
                fresh.MarkFaulted(ex.Message);
                await uow.HistoricalDataCollectionJobs.UpdateAsync(fresh);
                await uow.SaveChangesAsync(CancellationToken.None);
            }

            throw;
        }
    }
}