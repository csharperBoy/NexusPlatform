using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;

namespace Scheduler.Infrastructure.Jobs;

/// <summary>
/// بعد از Application Started، Recovery را در Background اجرا می‌کند.
/// چرا Background؟ چون Hangfire Server باید آماده باشد قبل از Schedule.
/// </summary>
public class RecurringScheduleStartupService : IHostedService
{
    private readonly IServiceProvider _rootSp;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<RecurringScheduleStartupService> _logger;

    public RecurringScheduleStartupService(
        IServiceProvider rootSp,
        IHostApplicationLifetime lifetime,
        ILogger<RecurringScheduleStartupService> logger)
    {
        _rootSp = rootSp;
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _lifetime.ApplicationStarted.Register(OnStarted);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private void OnStarted()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                // یک تأخیر کوچک تا Hangfire workerها کامل ready شوند
                await Task.Delay(TimeSpan.FromSeconds(3));

                using var scope = _rootSp.CreateScope();
                var recovery = scope.ServiceProvider
                    .GetRequiredService<IRecurringScheduleRecovery>();

                await recovery.RecoverAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "RecurringScheduleStartupService: recovery failed.");
            }
        });
    }
}