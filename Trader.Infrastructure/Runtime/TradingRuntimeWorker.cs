using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Runtime;

/// <summary>
/// BackgroundService اصلی Runtime.
/// در هر Tick، یک Scope جدید می‌سازد و Stageها را به ترتیب اجرا می‌کند.
/// خطای هر Tick، Worker را نمی‌کشد.
/// </summary>
public sealed class TradingRuntimeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TradingRuntimeOptions> _options;
    private readonly ILogger<TradingRuntimeWorker> _logger;

    public TradingRuntimeWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingRuntimeOptions> options,
        ILogger<TradingRuntimeWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TradingRuntimeWorker starting. Heartbeat={Heartbeat} OrderLifecycle={OrderLifecycle}",
            _options.CurrentValue.HeartbeatInterval,
            _options.CurrentValue.OrderLifecycleInterval);

        // ─── Startup Recovery ───
        try
        {
            await RunStartupRecoveryAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Startup recovery failed. Worker will continue, but runs may not be restored.");
        }

        // ─── Heartbeat Loop ───
        await RunHeartbeatLoopAsync(stoppingToken);

        _logger.LogInformation("TradingRuntimeWorker stopped.");
    }

    /* ═══════════════════ Heartbeat ═══════════════════ */

    private async Task RunHeartbeatLoopAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.CurrentValue.HeartbeatInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Tick failed. Worker continues on next interval.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Stage 1
            await sp.GetRequiredService<IMarketDataIngestionStage>().RunOnceAsync(ct);

            // Stage 2
            await sp.GetRequiredService<IStrategyEvaluationStage>().RunOnceAsync(ct);

            // Stage 3
            await sp.GetRequiredService<IExecutionStage>().RunOnceAsync(ct);

            // Stage 4 (با interval داخلی خودش)
            await sp.GetRequiredService<IOrderLifecycleStage>().RunOnceAsync(ct);
        }
        finally
        {
            stopwatch.Stop();

            // ─── Flush رویدادها — حتی اگر Stageها Fail شوند ───
            try
            {
                var eventLogger = sp.GetService<ITradingEventLogger>();
                if (eventLogger is not null)
                    await eventLogger.FlushAsync(CancellationToken.None);
            }
            catch (Exception flushEx)
            {
                _logger.LogError(flushEx,
                    "Failed to flush trading events at end of tick.");
            }

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Tick took {ElapsedMs} ms — slower than expected.",
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }

    /* ═══════════════════ Startup Recovery ═══════════════════ */

    private async Task RunStartupRecoveryAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var recovery = sp.GetService<IRuntimeRecoveryService>();
        if (recovery is null)
        {
            _logger.LogDebug("No IRuntimeRecoveryService registered; skipping recovery.");
            return;
        }

        await recovery.RecoverAsync(ct);
    }
}