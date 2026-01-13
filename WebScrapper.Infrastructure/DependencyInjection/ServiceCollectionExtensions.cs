using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebScrapper.Infrastructure.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WebScrapper_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            //var migrationsAssembly = typeof(SampleDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            //services.AddDbContext<SampleDbContext>((serviceProvider, options) =>
            //{
            //    options.UseSqlServer(conn, b =>
            //    {
            //        // تعیین Assembly محل Migrationها
            //        b.MigrationsAssembly(migrationsAssembly);

            //        // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
            //        b.MigrationsHistoryTable("__SampleMigrationsHistory", "sample");
            //    });
            //});

            //services.AddScoped<IUnitOfWork<SampleDbContext>, EfUnitOfWork<SampleDbContext>>();
            //// 📌 رجیستر Repository مبتنی بر Specification
            //services.AddScoped<ISpecificationRepository<SampleEntity, Guid>, EfSpecificationRepository<SampleDbContext, SampleEntity, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            //var registration = services.BuildServiceProvider()
                                       //.GetRequiredService<IOutboxProcessorRegistration>();
            //registration.AddOutboxProcessor<SampleDbContext>(services);

            return services;
        }
    }
}

