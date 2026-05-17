using Base.Application.Interfaces.Processor;
using Base.Application.Interfaces.Service;
using Base.Domain.Entities;
using Base.Infrastructure.Data;
using Base.Infrastructure.Processor;
using Base.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Base.PublicService;
using Core.Application.Abstractions.Events;
using Core.Application.Helper;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Base.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Base_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(BaseDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Base
            services.AddDbContext<BaseDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "base"
                    b.MigrationsHistoryTable("__BaseMigrationsHistory", "base");
                });
            });


            services.AddScoped<MenuService>();

            services.AddScoped<IMenuInternalService>(sp => sp.GetRequiredService<MenuService>());


            services.AddScoped<IMenuPublicService>(sp => sp.GetRequiredService<MenuService>());

            services.AddScoped<IRepository<BaseDbContext, Menu, Guid>, EfRepository<BaseDbContext, Menu, Guid>>();

            services.AddScoped<ISpecificationRepository<Menu, Guid>, EfSpecificationRepository<BaseDbContext, Menu, Guid>>();

            services.AddScoped<IMenuProcessor, MenuProcessor>();

            services.AddScoped<IUnitOfWork<BaseDbContext>, EfUnitOfWork<BaseDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            //services.AddScoped<ISpecificationRepository<BaseEntity, Guid>, EfSpecificationRepository<BaseDbContext, BaseEntity, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            //if (ModuleHelper.IsActive(Core.Domain.Enums.ModuleEnum.Event))
            //{
                // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
                var registration = services.BuildServiceProvider()
                                           .GetRequiredService<IOutboxProcessorRegistration>();
                registration.AddOutboxProcessor<BaseDbContext>(services);
            //}

            return services;
        }
    }
}
