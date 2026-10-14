using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Strategies;

namespace Trader.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
      
        public static IServiceCollection Trader_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            services.AddSingleton<IStrategyFactory, StrategyFactory>(); // در گام بعد پیاده می‌شود
            return services;
        }
    }
}