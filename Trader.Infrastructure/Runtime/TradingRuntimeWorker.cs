using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Health;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Runtime;
using Trader.Domain.Enums;

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
    private readonly IRuntimeHealthState _healthState;

    private readonly ILogger<TradingRuntimeWorker> _logger;

    public TradingRuntimeWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingRuntimeOptions> options,
    IRuntimeHealthState healthState,       
        ILogger<TradingRuntimeWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _healthState = healthState;
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
        _healthState.MarkTickNow();
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
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "TradingRuntimeWorker graceful shutdown started.");

        // ─── Log ShutdownStarted (fresh scope) ───
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var eventLogger = scope.ServiceProvider.GetService<ITradingEventLogger>();

            if (eventLogger is not null)
            {
                eventLogger.Log(
                    stage: TradingStage.System,
                    type: TradingEventType.ShutdownStarted,
                    severity: TradingEventSeverity.Info,
                    message: "خاموش‌شدن سیستم شروع شد. در حال اتمام Tick جاری...",
                    payload: new
                    {
                        startedAt = DateTime.UtcNow,
                    });

                await eventLogger.FlushAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log ShutdownStarted event; continuing.");
        }

        // ─── Wait for current Tick + loop to break ───
        // base.StopAsync cancels stoppingToken and awaits _executeTask.
        // HostOptions.ShutdownTimeout enforces upper bound.
        var shutdownStart = DateTime.UtcNow;

        try
        {
            await base.StopAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Shutdown cancelled by host timeout after {ElapsedMs}ms.",
                (DateTime.UtcNow - shutdownStart).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during StopAsync after {ElapsedMs}ms.",
                (DateTime.UtcNow - shutdownStart).TotalMilliseconds);
        }

        var elapsedMs = (long)(DateTime.UtcNow - shutdownStart).TotalMilliseconds;

        // ─── Log ShutdownCompleted (fresh scope) ───
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var eventLogger = scope.ServiceProvider.GetService<ITradingEventLogger>();

            if (eventLogger is not null)
            {
                eventLogger.Log(
                    stage: TradingStage.System,
                    type: TradingEventType.ShutdownCompleted,
                    severity: TradingEventSeverity.Info,
                    message: $"خاموش‌شدن سیستم کامل شد. مدت: {elapsedMs}ms.",
                    payload: new
                    {
                        elapsedMs,
                        completedAt = DateTime.UtcNow,
                    });

                await eventLogger.FlushAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log ShutdownCompleted event.");
        }

        _logger.LogInformation(
            "TradingRuntimeWorker graceful shutdown completed in {ElapsedMs}ms.",
            elapsedMs);
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