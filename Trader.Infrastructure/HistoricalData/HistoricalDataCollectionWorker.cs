using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Health;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.HistoricalData;

/// <summary>
/// Polls DB for Pending HistoricalDataCollectionJobs and processes them sequentially.
/// </summary>
public sealed class HistoricalDataCollectionWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRuntimeHealthState _healthState;

    private readonly ILogger<HistoricalDataCollectionWorker> _logger;

    public HistoricalDataCollectionWorker(
        IServiceScopeFactory scopeFactory,
    IRuntimeHealthState healthState,
        ILogger<HistoricalDataCollectionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _healthState = healthState;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HistoricalDataCollectionWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker iteration failed.");
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

        _logger.LogInformation("HistoricalDataCollectionWorker stopped.");
    }

    private async Task ProcessPendingJobsAsync(CancellationToken ct)
    {

        _healthState.MarkHistoricalPollNow();   
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var collector = scope.ServiceProvider.GetRequiredService<IHistoricalDataCollector>();

        var pending = (await uow.HistoricalDataCollectionJobs.GetAllAsync())
            .Where(j => j.Status == CollectionJobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .ToList();

        if (pending.Count == 0) return;

        _logger.LogInformation("Found {Count} pending collection jobs.", pending.Count);

        foreach (var job in pending)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await collector.RunAsync(job.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobId} threw unhandled exception.", job.Id);
            }
        }
    }
}