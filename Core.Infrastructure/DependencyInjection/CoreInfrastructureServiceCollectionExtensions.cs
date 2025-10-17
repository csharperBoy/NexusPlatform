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
        
        public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            // سرویس‌های پایه
            services.AddHttpContextAccessor();

            // Repositoryها
            services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
            services.AddScoped(typeof(ISpecificationRepository<,>), typeof(EfSpecificationRepository<,>));

            // Event Bus
            services.AddScoped<IEventBus, MediatorEventBus>();

            // Caching
            // یکی از کانفیگ های زیر رو ثبت میکنیم
            //1. for user from Radis
            #region Redis
            // for this install  Microsoft.Extensions.Caching.StackExchangeRedis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });
            services.AddScoped<ICacheService, RedisCacheService>();
            #endregion
            //1. for user from MemoryCache
            #region MemoryCache
            //services.AddMemoryCache();
            //services.AddScoped<ICacheService, MemoryCacheService>();
            #endregion

            // Validation Pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            
            return services;
        }

    }
}
