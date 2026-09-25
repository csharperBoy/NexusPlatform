using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Domain.Entities;
using Trader.Infrastructure.Brokers;
using Trader.Infrastructure.Brokers.EasyTrader;
using Trader.Infrastructure.Data;
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
            services.AddScoped<
                IRepository<TraderDbContext, TraderAccount, Guid>,
                EfRepository<TraderDbContext, TraderAccount, Guid>>();

            services.AddScoped<
                IRepository<TraderDbContext, TraderSymbol, Guid>,
                EfRepository<TraderDbContext, TraderSymbol, Guid>>();

            services.AddScoped<
                IRepository<TraderDbContext, SchedulePlan, Guid>,
                EfRepository<TraderDbContext, SchedulePlan, Guid>>();

            services.AddScoped<
                IRepository<TraderDbContext, ScheduledOrder, Guid>,
                EfRepository<TraderDbContext, ScheduledOrder, Guid>>();

            services.AddScoped<
                IRepository<TraderDbContext, ExecutionLog, Guid>,
                EfRepository<TraderDbContext, ExecutionLog, Guid>>();

            /* ═══════════ Specification Repositories ═══════════ */
            services.AddScoped<
                ISpecificationRepository<TraderAccount, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderAccount, Guid>>();

            services.AddScoped<
                ISpecificationRepository<TraderSymbol, Guid>,
                EfSpecificationRepository<TraderDbContext, TraderSymbol, Guid>>();

            services.AddScoped<
                ISpecificationRepository<SchedulePlan, Guid>,
                EfSpecificationRepository<TraderDbContext, SchedulePlan, Guid>>();

            /* ═══════════ Unit of Work ═══════════ */
            services.AddScoped<IUnitOfWork<TraderDbContext>, EfUnitOfWork<TraderDbContext>>();

            /* ═══════════ Broker Clients ═══════════ */
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            services.AddSingleton<EasyTraderBrokerClient>();
            services.AddSingleton<IBrokerClientFactory, BrokerClientFactory>();

            /* ═══════════ Command + Query Services ═══════════ */

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

            return services;
        }
    }
}