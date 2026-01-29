using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Trader.Server.Collector.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
         public static IServiceCollection TraderServer_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            return services;
        }
    }
}