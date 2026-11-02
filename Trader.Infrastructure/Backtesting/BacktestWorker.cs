using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

/// <summary>
/// Polls DB for Pending BacktestRuns and runs them sequentially.
/// </summary>
public sealed class BacktestWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BacktestWorker> _logger;

    public BacktestWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<BacktestWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BacktestWorker started.");

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
                _logger.LogError(ex, "BacktestWorker iteration failed.");
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

        _logger.LogInformation("BacktestWorker stopped.");
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var engine = scope.ServiceProvider.GetRequiredService<IBacktestEngine>();

        var pending = (await uow.BacktestRuns.GetAllAsync())
            .Where(b => b.Status == BacktestStatus.Pending)
            .OrderBy(b => b.CreatedAt)
            .ToList();

        if (pending.Count == 0) return;

        _logger.LogInformation("Found {Count} pending backtest runs.", pending.Count);

        foreach (var run in pending)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await engine.RunAsync(run.Id, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "BacktestRun {Id} threw unhandled exception.", run.Id);
            }
        }
    }
}