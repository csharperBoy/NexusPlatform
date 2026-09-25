using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Application.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Application.Scheduler;
using Trader.Application.Scheduler.Services;
using Trader.Domain.Entities;
using Trader.Infrastructure.Brokers;
using Trader.Infrastructure.Brokers.EasyTrader;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Scheduler;
using Trader.Infrastructure.Services;

namespace Trader.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Trader_AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection missing");

            /* ═══════════ DbContext ═══════════ */
            var migrationsAssembly = typeof(TraderDbContext).Assembly.GetName().Name;
            services.AddDbContext<TraderDbContext>((sp, opts) =>
            {
                opts.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__TraderMigrationsHistory", "trader");
                });
            });

            /* ═══════════ Repositories ═══════════ */
            services.AddScoped<IRepository<TraderDbContext, TraderAccount, Guid>,
                EfRepository<TraderDbContext, TraderAccount, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, TraderSymbol, Guid>,
                EfRepository<TraderDbContext, TraderSymbol, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, SchedulePlan, Guid>,
                EfRepository<TraderDbContext, SchedulePlan, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, ScheduledOrder, Guid>,
                EfRepository<TraderDbContext, ScheduledOrder, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, ExecutionLog, Guid>,
                EfRepository<TraderDbContext, ExecutionLog, Guid>>();

            services.AddScoped<ISpecificationRepository<TraderAccount, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderAccount, Guid>>();
            services.AddScoped<ISpecificationRepository<TraderSymbol, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderSymbol, Guid>>();
            services.AddScoped<ISpecificationRepository<SchedulePlan, Guid>,
                EfSpecificationRepository<TraderDbContext, SchedulePlan, Guid>>();

            services.AddScoped<IUnitOfWork<TraderDbContext>,
                EfUnitOfWork<TraderDbContext>>();

            /* ═══════════ Broker Clients ═══════════ */
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            services.AddSingleton<EasyTraderBrokerClient>();
            services.AddSingleton<IBrokerClientFactory, BrokerClientFactory>();

            /* ═══════════ Services ═══════════ */

            // Account
            services.AddScoped<AccountService>();
            services.AddScoped<IAccountCommandService>(
                sp => sp.GetRequiredService<AccountService>());
            services.AddScoped<IAccountQueryService>(
                sp => sp.GetRequiredService<AccountService>());

            // Symbol
            services.AddScoped<SymbolService>();
            services.AddScoped<ISymbolCommandService>(
                sp => sp.GetRequiredService<SymbolService>());
            services.AddScoped<ISymbolQueryService>(
                sp => sp.GetRequiredService<SymbolService>());

            // SchedulePlan
            services.AddScoped<SchedulePlanService>();
            services.AddScoped<ISchedulePlanCommandService>(
                sp => sp.GetRequiredService<SchedulePlanService>());
            services.AddScoped<ISchedulePlanQueryService>(
                sp => sp.GetRequiredService<SchedulePlanService>());

            // ServerClock (Singleton — چون diff رو نگه‌می‌داره و از scope factory استفاده می‌کنه)
            services.AddSingleton<ServerClockService>();
            services.AddSingleton<IServerClockCommandService>(
                sp => sp.GetRequiredService<ServerClockService>());
            services.AddSingleton<IServerClockQueryService>(
                sp => sp.GetRequiredService<ServerClockService>());

            // PlanExecutor
            services.AddScoped<IPlanExecutor, PlanExecutor>();

            /* ═══════════ Scheduler Job Handler ═══════════ */
            services.AddScheduledJobHandler<PlanExecutionPayload, PlanExecutionJobHandler>();

            return services;
        }
    }
}