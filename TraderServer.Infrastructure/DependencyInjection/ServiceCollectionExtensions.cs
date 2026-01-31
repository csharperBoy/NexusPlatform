using BrokerageOperations.Infrastructure.DependencyInjection;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TraderServer.Application.Interface;
using TraderServer.Domain.Entities;
using TraderServer.Infrastructure.Service;

namespace TraderServer.Infrastructure.DependencyInjection
{
 
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TraderServer_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.BrokerageOperations_AddInfrastructure(configuration);
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(TraderDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            services.AddDbContext<TraderDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__TraderMigrationsHistory", "Trader");
                });
            });

            services.AddScoped<IUnitOfWork<TraderDbContext>, EfUnitOfWork<TraderDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<ISpecificationRepository<Option, Guid>, EfSpecificationRepository<TraderDbContext, Option, Guid>>();
            services.AddScoped<ISpecificationRepository<OptionContract, Guid>, EfSpecificationRepository<TraderDbContext, OptionContract, Guid>>();
            services.AddScoped<ISpecificationRepository<SnapShotFromOptionTrading, Guid>, EfSpecificationRepository<TraderDbContext, SnapShotFromOptionTrading, Guid>>();
            services.AddScoped<ISpecificationRepository<SnapShotFromStockTrading, Guid>, EfSpecificationRepository<TraderDbContext, SnapShotFromStockTrading, Guid>>();
            services.AddScoped<ISpecificationRepository<Stock, Guid>, EfSpecificationRepository<TraderDbContext, Stock, Guid>>();
            services.AddScoped<ISpecificationRepository<StockFundPortfolio, Guid>, EfSpecificationRepository<TraderDbContext, StockFundPortfolio, Guid>>();

            services.AddScoped<ICollectorService, CollectorService>();
            services.AddScoped<IStockService, StockService>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<TraderDbContext>(services);

            return services;
        }
    }
}

