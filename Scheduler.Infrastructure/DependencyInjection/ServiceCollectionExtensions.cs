using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Application.Abstractions;
using Scheduler.Application.DependencyInjection;
using Scheduler.Application.Models;
using Scheduler.Infrastructure.Data;
using Scheduler.Infrastructure.Jobs;
using Scheduler.Infrastructure.Data;
using Scheduler.Infrastructure.Services;

namespace Scheduler.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ثبت زیرساخت Scheduler.
    /// امضای استاندارد — بدون پارامتر اضافی.
    /// Payloadها خودکار از assemblyهای لود شده + registrationهای صریح کشف میشن.
    /// </summary>
    public static IServiceCollection Scheduler_AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection missing");

        /* ─── Options ─── */
        services.Configure<SchedulerOptions>(
            configuration.GetSection(SchedulerOptions.SectionName));

        var options = configuration
            .GetSection(SchedulerOptions.SectionName)
            .Get<SchedulerOptions>() ?? new SchedulerOptions();

        /* ─── DbContext ─── */
        var migrationsAssembly = typeof(SchedulerDbContext).Assembly.GetName().Name;
        services.AddDbContext<SchedulerDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(conn, b =>
            {
                b.MigrationsAssembly(migrationsAssembly);
                b.MigrationsHistoryTable("__SchedulerMigrationsHistory", "scheduler");
            });
        });

        /* ─── JobTypeRegistry (Singleton با Auto-Scan + Manual Registration) ─── */
        services.AddSingleton<JobTypeRegistry>(sp =>
        {
            var registry = new JobTypeRegistry();
            registry.ScanAllAssemblies();

            // اعمال registrationهای صریح (از AddScheduledJobHandler)
            var manual = sp.GetServices<ScheduledJobPayloadRegistration>();
            foreach (var reg in manual)
            {
                registry.Register(reg.PayloadType);
            }

            return registry;
        });

        /* ─── Dispatcher ─── */
        services.AddScoped<HangfireJobDispatcher>();

        /* ─── SchedulerService ─── */
        services.AddScoped<ISchedulerService, HangfireSchedulerService>();

        /* ─── Hangfire storage ─── */
        services.AddHangfire((sp, cfg) =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSqlServerStorage(conn, new SqlServerStorageOptions
               {
                   SchemaName = "scheduler",
                   PrepareSchemaIfNecessary = true,
                   QueuePollInterval = TimeSpan.FromSeconds(
                       options.SchedulePollIntervalSeconds),
               });
        });

        /* ─── Hangfire server ─── */
        services.AddHangfireServer((sp, opts) =>
        {
            opts.Queues = new[] { options.DefaultQueue };
            opts.SchedulePollingInterval = TimeSpan.FromSeconds(
                options.SchedulePollIntervalSeconds);
            if (options.WorkerCount > 0)
                opts.WorkerCount = options.WorkerCount;
        });

        return services;
    }
}