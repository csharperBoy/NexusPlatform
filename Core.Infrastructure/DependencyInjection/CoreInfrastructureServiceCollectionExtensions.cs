using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Core.Infrastructure.Repositories;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Application.Behaviors;
using Core.Infrastructure.Caching;
using Core.Infrastructure.Events;
using MediatR;

namespace Core.Infrastructure.DependencyInjection
{
    public static class CoreInfrastructureServiceCollectionExtensions
    {
        
        public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
        {

            // سرویس‌های پایه
            services.AddHttpContextAccessor();

            // Repositoryها
            services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
            services.AddScoped(typeof(ISpecificationRepository<,>), typeof(EfSpecificationRepository<,>));

            // Event Bus
            services.AddScoped<IEventBus, MediatorEventBus>();

            // Caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });
            services.AddScoped<ICacheService, RedisCacheService>();

            // Validation Pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            
            return services;
        }

    }
}
