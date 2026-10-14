using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Application.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Scheduler;
using Trader.Application.Scheduler.Services;
using Trader.Application.Services;
using Trader.Application.Strategies;
using Trader.Application.Strategies.Rsi;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.ExternalApiClient.DependencyInjection;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers;
using Trader.Infrastructure.Brokers.EasyTrader;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Runtime;
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
            // ─── Runtime Singleton ───
            services.AddSingleton<IRunExecutionContextCache, RunExecutionContextCache>();

            // ─── Runtime Scoped ───
            services.AddScoped<ITradingRunManager, TradingRunManager>();

            // ─── Strategy Factory ───
            services.AddSingleton<IStrategyFactory, StrategyFactory>();
            services.AddSingleton<IStrategyRegistration, RsiStrategyRegistration>();
            // هر Strategy جدید: services.AddSingleton<IStrategyRegistration, XxxStrategyRegistration>();

            // ─── Application Services ───
            services.AddScoped<IInstrumentCommandService, InstrumentService>();
            services.AddScoped<IInstrumentQueryService, InstrumentService>();
            services.AddScoped<IInstrumentOrderPresetService, InstrumentOrderPresetService>();
            services.AddScoped<ITradingRunCommandService, TradingRunService>();
            services.AddScoped<ITradingRunQueryService, TradingRunService>();
            services.AddScoped<ISignalQueryService, SignalQueryService>();
            services.AddScoped<IPositionQueryService, PositionQueryService>();
            services.AddScoped<IStrategyQueryService, StrategyQueryService>();

            // ─── Unit of Work ───
            services.AddScoped<ITraderUnitOfWork<TraderDbContext>, TraderUnitOfWork>();

            // ─── Repositories ───
            services.AddScoped<IRepository<TraderDbContext, Instrument, Guid>, EfRepository<TraderDbContext, Instrument, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, InstrumentOrderPreset, Guid>, EfRepository<TraderDbContext, InstrumentOrderPreset, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, TradingRun, Guid>, EfRepository<TraderDbContext, TradingRun, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, Signal, Guid>, EfRepository<TraderDbContext, Signal, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, OrderIntent, Guid>, EfRepository<TraderDbContext, OrderIntent, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, BrokerOrder, Guid>, EfRepository<TraderDbContext, BrokerOrder, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, Position, Guid>, EfRepository<TraderDbContext, Position, Guid>>();

            // ─── Spec Repositories ───
            services.AddScoped<ISpecificationRepository<Instrument, Guid>, EfSpecificationRepository<TraderDbContext, Instrument, Guid>>();
            services.AddScoped<ISpecificationRepository<InstrumentOrderPreset, Guid>, EfSpecificationRepository<TraderDbContext, InstrumentOrderPreset, Guid>>();
            services.AddScoped<ISpecificationRepository<TradingRun, Guid>, EfSpecificationRepository<TraderDbContext, TradingRun, Guid>>();
            services.AddScoped<ISpecificationRepository<Signal, Guid>, EfSpecificationRepository<TraderDbContext, Signal, Guid>>();
            services.AddScoped<ISpecificationRepository<OrderIntent, Guid>, EfSpecificationRepository<TraderDbContext, OrderIntent, Guid>>();
            services.AddScoped<ISpecificationRepository<BrokerOrder, Guid>, EfSpecificationRepository<TraderDbContext, BrokerOrder, Guid>>();
            services.AddScoped<ISpecificationRepository<Position, Guid>, EfSpecificationRepository<TraderDbContext, Position, Guid>>();
            /* ═══════════ Broker Clients ═══════════ */
            services.Trader_ExternalApiClient_DependencyInjection(configuration);

            //services.AddScoped<EasyTraderBrokerClient>();

            services.AddEasyTraderBroker();
            //services.AddBroker<AgahBrokerClient>(BrokerType.Agah);

            services.AddScoped<IBrokerClientFactory, BrokerClientFactory>();

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

            // ExecutionLog
            services.AddScoped<ExecutionLogService>();
            services.AddScoped<IExecutionLogQueryService>(
                sp => sp.GetRequiredService<ExecutionLogService>());

            /* ═══════════ Scheduler Job Handler ═══════════ */
            services.AddScheduledJobHandler<PlanExecutionPayload, PlanExecutionJobHandler>();


            /* ═══════════ Background Worker For Sync Clock ═══════════ */
            services.Configure<ClockSyncOptions>(
                configuration.GetSection(ClockSyncOptions.SectionName));

            services.AddHostedService<ClockSyncBackgroundService>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

           
            return services;
        }
    }
}