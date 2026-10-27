using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.Navigation.PublicService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Domain.ValueObjects;
using Trader.Application.Abstractions;
using Trader.Application.Runtime;
using Trader.Application.Scheduler;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Data;

public class ModuleInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModuleInitializer> _logger;
    private readonly IConfiguration _configuration;

    public ModuleInitializer(
        IServiceProvider serviceProvider,
        ILogger<ModuleInitializer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            _logger.LogInformation("Starting Trader module initialization...");

            // ─── Authorization + Navigation Seed ───
            var resourceService = services.GetRequiredService<IResourcePublicService>();
            var permissionService = services.GetRequiredService<IPermissionPublicService>();
            var roleService = services.GetRequiredService<IRolePublicService>();
            var menuService = services.GetRequiredService<IMenuPublicService>();

            await TraderSeedData.SeedTraderForAuthorizationAsync(
                resourceService, permissionService, roleService, _logger);

            await TraderSeedData.SeedTradersForNavigationAsync(menuService, _logger);

            // ─── Trading Schedules Seed ───
            var uow = services.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
            await TraderSeedData.SeedTradingSchedulesAsync(uow, _logger, cancellationToken);

            // ─── NEW: Register Retention Recurring Schedule ───
            await EnsureRetentionRecurringScheduleAsync(services, cancellationToken);

            _logger.LogInformation("Trader module initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the Trader module");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /* ═══════════════════ Retention Recurring Schedule ═══════════════════ */

    private async Task EnsureRetentionRecurringScheduleAsync(
        IServiceProvider sp, CancellationToken ct)
    {
        try
        {
            var scheduler = sp.GetService<ISchedulerService>();
            if (scheduler is null)
            {
                _logger.LogWarning(
                    "ISchedulerService not registered. " +
                    "Retention Recurring Schedule will not be created.");
                return;
            }

            // ─── Idempotency: check if already exists ───
            var existing = await scheduler.QueryRecurringAsync(
                new RecurringSchedulerQuery(JobType: RetentionCleanupPayload.Key), ct);

            if (existing.Count > 0)
            {
                _logger.LogInformation(
                    "Retention RecurringSchedule already exists (Id: {Id}). Skipping.",
                    existing[0].Id);
                return;
            }

            // ─── Read RetentionDays from Options ───
            var loggingOptions = sp.GetRequiredService<IOptionsMonitor<TradingLoggingOptions>>();
            var retentionDays = loggingOptions.CurrentValue.RetentionDays;

            // ─── Compute first run time ───
            var tz = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
            var startAt = ComputeNextRunAt2330Tehran(tz);

            // ─── Schedule Recurring ───
            var id = await scheduler.ScheduleRecurringAsync(
                name: "trader-event-retention",
                payload: new RetentionCleanupPayload { RetentionDays = retentionDays },
                pattern: RecurrencePattern.Daily(),
                startAt: startAt,
                timeZone: tz,
                ct: ct);

            _logger.LogInformation(
                "Retention RecurringSchedule created: Id={Id} FirstRun={StartAt:O} " +
                "RetentionDays={Days}",
                id, startAt, retentionDays);
        }
        catch (Exception ex)
        {
            // Don't rethrow — module init should succeed even if this fails
            _logger.LogError(ex,
                "Failed to register Retention RecurringSchedule. " +
                "Retention cleanup will NOT run automatically.");
        }
    }

    /// <summary>
    /// زمان اولین اجرا: امروز ۲۳:۳۰ تهران (اگر نگذشته) — وگرنه فردا ۲۳:۳۰.
    /// </summary>
    private static DateTimeOffset ComputeNextRunAt2330Tehran(TimeZoneInfo tz)
    {
        var nowUtc = DateTime.UtcNow;
        var nowTehran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

        var todayRun = nowTehran.Date.AddHours(23).AddMinutes(30);

        DateTime targetTehran;
        if (nowTehran < todayRun)
            targetTehran = todayRun;             // امروز
        else
            targetTehran = todayRun.AddDays(1);  // فردا

        var unspecified = DateTime.SpecifyKind(targetTehran, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }
}