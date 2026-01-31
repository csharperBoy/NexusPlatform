using BrokerageOperations.Application.Interface;
using BrokerageOperations.Infrastructure.Service;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebScrapper.Infrastructure.DependencyInjection;

namespace BrokerageOperations.Infrastructure.DependencyInjection
{
 
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection BrokerageOperations_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.WebScrapper_AddInfrastructure(configuration);
            // 📌 گرفتن Connection String از تنظیمات
            /* var conn = configuration.GetConnectionString("DefaultConnection");
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
             */
            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<IBrokerageFactory, BrokerageFactory>();
            services.AddScoped<IBrokerageOperationsService, EasyTraderService>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            //registration.AddOutboxProcessor<TraderDbContext>(services);

            return services;
        }
    }
}

