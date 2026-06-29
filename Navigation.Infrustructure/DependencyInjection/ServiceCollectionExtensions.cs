using Navigation.Application.Interfaces.Processor;
using Navigation.Application.Interfaces.Service;
using Navigation.Domain.Entities;
using Navigation.Infrastructure.Data;
using Navigation.Infrastructure.Processor;
using Navigation.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Events;
using Core.Application.Helper;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Abstractions.Navigation.PublicService;


namespace Navigation.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Navigation_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(NavigationDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Base
            services.AddDbContext<NavigationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "navigation"
                    b.MigrationsHistoryTable("__NavigationMigrationsHistory", "navigation");
                });
            });


            services.AddScoped<MenuService>();

            services.AddScoped<IMenuInternalService>(sp => sp.GetRequiredService<MenuService>());


            services.AddScoped<IMenuPublicService>(sp => sp.GetRequiredService<MenuService>());

            services.AddScoped<IRepository<NavigationDbContext, Menu, Guid>, EfRepository<NavigationDbContext, Menu, Guid>>();

            services.AddScoped<ISpecificationRepository<Menu, Guid>, EfSpecificationRepository<NavigationDbContext, Menu, Guid>>();

            services.AddScoped<IMenuProcessor, MenuProcessor>();

            services.AddScoped<IUnitOfWork<NavigationDbContext>, EfUnitOfWork<NavigationDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            //services.AddScoped<ISpecificationRepository<BaseEntity, Guid>, EfSpecificationRepository<BaseDbContext, BaseEntity, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            //if (ModuleHelper.IsActive(Core.Domain.Enums.ModuleEnum.Event))
            //{
                // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
                var registration = services.BuildServiceProvider()
                                           .GetRequiredService<IOutboxProcessorRegistration>();
                registration.AddOutboxProcessor<NavigationDbContext>(services);
            //}

            return services;
        }
    }
}
