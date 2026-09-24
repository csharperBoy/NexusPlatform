using Core.Application.Abstractions.Security;
using Core.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.EasyTrader;

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

            services.AddDbContext<TraderDbContext>((sp, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__TraderMigrationsHistory", "trader");
                });
            });

            /* ─── EasyTrader Client ─── */
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            services.AddScoped<IEasyTraderClient, EasyTraderClient>();

            /* ─── Security ─── */
            services.AddDataProtection()
                .SetApplicationName("NexusPlatform.Trader");


            /* ─── UnitOfWork و Repositories ─── */
            // (فاز ۳)

            /* ─── Query Services ─── */
            // (فاز ۳)

            /* ─── Command Services ─── */
            // (فاز ۳)

            return services;
        }
    }
}