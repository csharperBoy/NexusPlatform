using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using EPM.Application.Abstractions;
using EPM.Domain.Entities;
using EPM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EPM.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Epm_AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection missing");

            /* ═══════════ DbContext ═══════════ */
            var migrationsAssembly = typeof(EpmDbContext).Assembly.GetName().Name;
            services.AddDbContext<EpmDbContext>((sp, opts) =>
            {
                opts.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__EpmMigrationsHistory", "epm");
                });

                // Split queries برای همه‌ی Queryها — ایمن برای TradingRun (چند Collection)
                //opts.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            /* ═══════════ Repositories — Direct ═══════════ */

            services.AddScoped<IRepository<EpmDbContext, User, Guid>,
                EfRepository<EpmDbContext, User, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, Indicator, Guid>,
                EfRepository<EpmDbContext, Indicator, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, IndicatorCategory, Guid>,
                EfRepository<EpmDbContext, IndicatorCategory, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, IndicatorValue, Guid>,
                EfRepository<EpmDbContext, IndicatorValue, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, Category, Guid>,
                EfRepository<EpmDbContext, Category, Guid>>();

            services.AddScoped<IRepository<EpmDbContext, LookUp, Guid>,
                EfRepository<EpmDbContext, LookUp, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, LookUpDestination, Guid>,
                EfRepository<EpmDbContext, LookUpDestination, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, LookUpValue, Guid>,
                EfRepository<EpmDbContext, LookUpValue, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, ClaimUserOnIndicator, Guid>,
                EfRepository<EpmDbContext, ClaimUserOnIndicator, Guid>>();
            services.AddScoped<IRepository<EpmDbContext, MapItem, Guid>,
                EfRepository<EpmDbContext, MapItem, Guid>>();
            /* ═══════════ Repositories — Specification ═══════════ */
            services.AddScoped<ISpecificationRepository<User, Guid>,
EfSpecificationRepository<EpmDbContext, User, Guid>>();
            services.AddScoped<ISpecificationRepository<Indicator, Guid>,
EfSpecificationRepository<EpmDbContext, Indicator, Guid>>();


            services.AddScoped<ISpecificationRepository<IndicatorCategory, Guid>,
EfSpecificationRepository<EpmDbContext, IndicatorCategory, Guid>>();
            services.AddScoped<ISpecificationRepository<IndicatorValue, Guid>,
EfSpecificationRepository<EpmDbContext, IndicatorValue, Guid>>();

            services.AddScoped<ISpecificationRepository<Category, Guid>,
                EfSpecificationRepository<EpmDbContext, Category, Guid>>();

            services.AddScoped<ISpecificationRepository<LookUp, Guid>,
                EfSpecificationRepository<EpmDbContext, LookUp, Guid>>();
            services.AddScoped<ISpecificationRepository<LookUpDestination, Guid>,
                EfSpecificationRepository<EpmDbContext, LookUpDestination, Guid>>();
            services.AddScoped<ISpecificationRepository<LookUpValue, Guid>,
                EfSpecificationRepository<EpmDbContext, LookUpValue, Guid>>();
            services.AddScoped<ISpecificationRepository<ClaimUserOnIndicator, Guid>,
                EfSpecificationRepository<EpmDbContext, ClaimUserOnIndicator, Guid>>();
            services.AddScoped<ISpecificationRepository<MapItem, Guid>,
                EfSpecificationRepository<EpmDbContext, MapItem, Guid>>();

            // ─── Unit of Work ───
            services.AddScoped<IEpmUnitOfWork<EpmDbContext>, EpmUnitOfWork>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();


            return services;
        }
    }
}