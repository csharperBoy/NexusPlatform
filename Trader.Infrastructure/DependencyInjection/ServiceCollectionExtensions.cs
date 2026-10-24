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
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Positions;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Indicators.Rsi;
using Trader.Application.Runtime;
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
using Trader.Infrastructure.Execution;
using Trader.Infrastructure.MarketData;
using Trader.Infrastructure.Positions;
using Trader.Infrastructure.Runtime;
using Trader.Infrastructure.Runtime.Recovery;
using Trader.Infrastructure.Runtime.Stages;
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

                // Split queries برای همه‌ی Queryها — ایمن برای TradingRun (چند Collection)
                //opts.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            /* ═══════════ Repositories — Direct ═══════════ */

            // Legacy
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

            // Trading System
            services.AddScoped<IRepository<TraderDbContext, Instrument, Guid>,
                EfRepository<TraderDbContext, Instrument, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, InstrumentOrderPreset, Guid>,
                EfRepository<TraderDbContext, InstrumentOrderPreset, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, TradingRun, Guid>,
                EfRepository<TraderDbContext, TradingRun, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, Signal, Guid>,
                EfRepository<TraderDbContext, Signal, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, OrderIntent, Guid>,
                EfRepository<TraderDbContext, OrderIntent, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, BrokerOrder, Guid>,
                EfRepository<TraderDbContext, BrokerOrder, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, OrderTrade, Guid>,
                EfRepository<TraderDbContext, OrderTrade, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, Position, Guid>,
                EfRepository<TraderDbContext, Position, Guid>>();

            services.AddScoped<IRepository<TraderDbContext, StrategyEvaluation, Guid>,
                EfRepository<TraderDbContext, StrategyEvaluation, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, MarketSnapshotRecord, Guid>,
                EfRepository<TraderDbContext, MarketSnapshotRecord, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, CandleRecord, Guid>,
   EfRepository<TraderDbContext, CandleRecord, Guid>>();
            services.AddScoped<IRepository<TraderDbContext, TradingSchedule, Guid>,
   EfRepository<TraderDbContext, TradingSchedule, Guid>>();

            /* ═══════════ Repositories — Specification ═══════════ */

            // Legacy
            services.AddScoped<ISpecificationRepository<TraderAccount, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderAccount, Guid>>();
            services.AddScoped<ISpecificationRepository<TraderSymbol, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderSymbol, Guid>>();
            services.AddScoped<ISpecificationRepository<SchedulePlan, Guid>,
                EfSpecificationRepository<TraderDbContext, SchedulePlan, Guid>>();
            services.AddScoped<ISpecificationRepository<ExecutionLog, Guid>,
                EfSpecificationRepository<TraderDbContext, ExecutionLog, Guid>>();

            // Trading System
            services.AddScoped<ISpecificationRepository<Instrument, Guid>,
                EfSpecificationRepository<TraderDbContext, Instrument, Guid>>();
            services.AddScoped<ISpecificationRepository<InstrumentOrderPreset, Guid>,
                EfSpecificationRepository<TraderDbContext, InstrumentOrderPreset, Guid>>();
            services.AddScoped<ISpecificationRepository<TradingRun, Guid>,
                EfSpecificationRepository<TraderDbContext, TradingRun, Guid>>();
            services.AddScoped<ISpecificationRepository<Signal, Guid>,
                EfSpecificationRepository<TraderDbContext, Signal, Guid>>();
            services.AddScoped<ISpecificationRepository<OrderIntent, Guid>,
                EfSpecificationRepository<TraderDbContext, OrderIntent, Guid>>();
            services.AddScoped<ISpecificationRepository<BrokerOrder, Guid>,
                EfSpecificationRepository<TraderDbContext, BrokerOrder, Guid>>();
            services.AddScoped<ISpecificationRepository<OrderTrade, Guid>,
                EfSpecificationRepository<TraderDbContext, OrderTrade, Guid>>();
            services.AddScoped<ISpecificationRepository<Position, Guid>,
                EfSpecificationRepository<TraderDbContext, Position, Guid>>();
            services.AddScoped<ISpecificationRepository<StrategyEvaluation, Guid>,
                EfSpecificationRepository<TraderDbContext, StrategyEvaluation, Guid>>();
            services.AddScoped<ISpecificationRepository<MarketSnapshotRecord, Guid>,
                EfSpecificationRepository<TraderDbContext, MarketSnapshotRecord, Guid>>();
            services.AddScoped<ISpecificationRepository<CandleRecord, Guid>,
                EfSpecificationRepository<TraderDbContext, CandleRecord, Guid>>();
            services.AddScoped<ISpecificationRepository<TradingSchedule, Guid>,
                EfSpecificationRepository<TraderDbContext, TradingSchedule, Guid>>();

            services.AddScoped<ICandlePersistenceService, CandlePersistenceService>();

            services.AddScoped<IUnitOfWork<TraderDbContext>,
                EfUnitOfWork<TraderDbContext>>();
            services.AddSingleton<IAccountBalanceCache, AccountBalanceCache>();
            // ─── Position Manager ───
            services.AddScoped<IPositionManager, PositionManager>();

            // ─── Stages ───
            services.AddScoped<IOrderLifecycleStage, OrderLifecycleStage>();
            // ─── Capital Allocators ───
            services.AddScoped<FixedCapitalAllocator>();
            services.AddScoped<DynamicCapitalAllocator>();
            services.AddScoped<ICapitalAllocator, CompositeCapitalAllocator>();

            // ─── Execution ───
            services.AddScoped<IPositionSizer, PercentPositionSizer>();
            services.AddScoped<IOrderIntentBuilder, OrderIntentBuilder>();
            services.AddScoped<IBrokerOrderDispatcher, BrokerOrderDispatcher>();
            services.AddScoped<IExecutionCoordinator, ExecutionCoordinator>();

            // ─── Stages ───
            services.AddScoped<IExecutionStage, ExecutionStage>();
            // ─── User Context Cache (Singleton) ───
            services.AddSingleton<IUserContextCache, UserContextCache>();
            // ─── Signal Channel (Singleton) ───
            services.AddSingleton<ISignalChannel, SignalChannel>();

            // ─── Candle Cache (Singleton) ───
            services.AddSingleton<ICandleCache, InMemoryCandleCache>();

            // ─── Runtime Stages ───
            services.AddScoped<IStrategyEvaluationStage, StrategyEvaluationStage>();
            // ─── Options ───
            services.Configure<TradingRuntimeOptions>(
                configuration.GetSection(TradingRuntimeOptions.SectionName));

            // ─── Session Cache (Singleton) ───
            // TODO: When moving to multi-instance, replace with RedisBrokerSessionCache.
            services.AddSingleton<IBrokerSessionCache, InMemoryBrokerSessionCache>();

            // ─── Session Provider (Scoped چون ISecretProtector و UoW Scoped هستند) ───
            services.AddScoped<IBrokerSessionProvider, BrokerSessionProvider>();

            // ─── Market Data (existing) ───
            services.AddSingleton<IMarketSnapshotStore, InMemoryMarketSnapshotStore>();
            services.AddScoped<IMarketDataProvider, BrokerMarketDataProvider>();
            services.AddScoped<IMarketDataProviderResolver, MarketDataProviderResolver>();

            services.AddScoped<ITradingScheduleQueryService, TradingScheduleService>();
            // ─── Runtime Stages ───
            services.AddScoped<IMarketDataIngestionStage, MarketDataIngestionStage>();
            // ─── Runtime Recovery ───
            services.AddScoped<IRuntimeRecoveryService, RuntimeRecoveryService>();

            // ─── Runtime Worker (HostedService) ───
            services.AddHostedService<TradingRuntimeWorker>();
            // ─── Runtime Singleton ───
            services.AddSingleton<IRunExecutionContextCache, RunExecutionContextCache>();

            // ─── Runtime Scoped ───
            services.AddScoped<ITradingRunManager, TradingRunManager>();

            // ─── Indicators ───
            services.AddSingleton<IIndicator<RsiConfig, decimal?>, RsiIndicator>();
            // ─── Market Session ───
            services.AddSingleton<ITradingScheduleCache, TradingScheduleCache>();
            services.AddSingleton<IMarketSessionProvider, IranMarketSessionProvider>();
            // ─── Strategy Factory ───
            services.AddTraderStrategies();
            services.AddSingleton<IStrategyFactory, StrategyFactory>();
            // Position Closer
            services.AddSingleton<IPositionCloser, PositionCloser>();

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