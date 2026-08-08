using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamBus.Application.Abstractions;
using StreamBus.Application.Options;
using StreamBus.Infrastructure.BackgroundServices;
using StreamBus.Infrastructure.Services;

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

            //services.AddTransient(typeof(IStreamBusClient<,>), typeof(GrpcStreamBusClient<,>));
            services.AddTransient(typeof(IStreamBusClient<,>), typeof(ResilientStreamBusClient<,>));
            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            //var registration = services.BuildServiceProvider()
            //.GetRequiredService<IOutboxProcessorRegistration>();
            //registration.AddOutboxProcessor<StreamBusDbContext>(services);

            return services;
        }
        public static IServiceCollection AddStreamBusConsumer<TRequest, TResponse>(
       this IServiceCollection services,
       Action<StreamConsumerOptions<TRequest, TResponse>> configureOptions)
       where TRequest : class
       where TResponse : class
        {
            var options = new StreamConsumerOptions<TRequest, TResponse>();
            configureOptions(options);

            services.AddSingleton(options);
            services.AddHostedService<StreamBusConsumerWorker<TRequest, TResponse>>();

            return services;
        }
    }
}

