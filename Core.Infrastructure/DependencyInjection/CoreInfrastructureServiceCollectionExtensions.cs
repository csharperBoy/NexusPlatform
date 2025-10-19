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
using Core.Application.Models;
using Core.Infrastructure.HealthChecks;

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

            // Health Checks
            services.AddScoped<IHealthCheckService, HealthCheckService>();

            #endregion
            //1. for user from MemoryCache
            #region MemoryCache
            //services.AddMemoryCache();
            //services.AddScoped<ICacheService, MemoryCacheService>();
            #endregion
            // پیکربندی کش
            ConfigureCaching(services, configuration);

            // Validation Pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            
            return services;
        }
        private static void ConfigureCaching(IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? new CacheSettings();
            var redisConnection = configuration.GetConnectionString("Redis");

            if (cacheSettings.UseRedis && !string.IsNullOrEmpty(redisConnection))
            {
                try
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnection;
                        options.InstanceName = cacheSettings.RedisInstanceName;
                    });
                    services.AddScoped<ICacheService, RedisCacheService>();

                    Console.WriteLine("✅ Redis Cache configured successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Redis configuration failed: {ex.Message}. Falling back to MemoryCache.");
                    ConfigureMemoryCache(services);
                }
            }
            else
            {
                ConfigureMemoryCache(services);
            }
        }

        private static void ConfigureMemoryCache(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
            Console.WriteLine("✅ In-Memory Cache configured");
        }

    }
}
