using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StreamBus.Infrastructure.DependencyInjection
{
    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection StreamBus_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
           
            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            //var registration = services.BuildServiceProvider()
                                       //.GetRequiredService<IOutboxProcessorRegistration>();
            //registration.AddOutboxProcessor<StreamBusDbContext>(services);

            return services;
        }
    }
}

