using Core.Application.Abstractions.Security;
using Core.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Infrastructure.Brokers;
using Trader.Infrastructure.Brokers.EasyTrader;
using Trader.Infrastructure.Data;

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

            /* ─── DbContext ─── */
            var migrationsAssembly = typeof(TraderDbContext).Assembly.GetName().Name;
            services.AddDbContext<TraderDbContext>((sp, opts) =>
            {
                opts.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__TraderMigrationsHistory", "trader");
                });
            });

            /* ─── Security ─── */
            services.AddDataProtection()
                .SetApplicationName("NexusPlatform.Trader");
            services.AddScoped<ISecretProtector, DataProtectionSecretProtector>();

            /* ─── Broker Clients ─── */
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            // Singleton چون stateless هستن (session پارامتره)
            services.AddSingleton<IBrokerClient, EasyTraderBrokerClient>();
            services.AddSingleton<IBrokerClientFactory, BrokerClientFactory>();

            /* ─── Command + Query Services ─── */
            // (فاز ۳)

            return services;
        }
    }
}