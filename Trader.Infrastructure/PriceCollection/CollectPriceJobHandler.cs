using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Scheduler;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.PriceCollection;

/// <summary>
/// Handler برای CollectPricePayload (D45, D47).
/// - Idempotent: اگر Job در DB Stopped/Faulted است، return.
/// - Dispatch بر اساس Mode.
/// - خطا → MarkFaulted → throw → Scheduler auto-pause @ 10 failures (D44).
/// </summary>
public sealed class CollectPriceJobHandler : IScheduledJobHandler<CollectPricePayload>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollectPriceJobHandler> _logger;

    public CollectPriceJobHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<CollectPriceJobHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(CollectPricePayload payload, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var uow = sp.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var job = await uow.PriceCollectionJobs.GetByIdAsync(payload.JobId);

        // ─── Idempotent guard (D45) ───
        if (job is null)
        {
            _logger.LogWarning(
                "CollectPriceJobHandler: job {Id} not found; skip.", payload.JobId);
            return;
        }

        if (job.State != PriceCollectionJobState.Running)
        {
            _logger.LogDebug(
                "CollectPriceJobHandler: job {Id} state={State}; skip.",
                payload.JobId, job.State);
            return;
        }

        var service = sp.GetRequiredService<IPriceCollectionService>();

        try
        {
            if (job.Mode == PriceCollectionMode.Backfill)
                await service.CollectBackfillAsync(job.Id, ct);
            else
                await service.CollectOnceAsync(job.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CollectPriceJobHandler failed: job={Id} mode={Mode}",
                job.Id, job.Mode);

            // Reload — collect method may have partially modified
            var fresh = await uow.PriceCollectionJobs.GetByIdAsync(job.Id);
            if (fresh is not null && fresh.State == PriceCollectionJobState.Running)
            {
                fresh.MarkFaulted(ex.Message);
                await uow.SaveChangesAsync(CancellationToken.None);
            }

            throw;   // → Scheduler retry/pause
        }
    }
}