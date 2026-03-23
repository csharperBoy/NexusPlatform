using Cach.Application.Models;
using Cach.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.Events;
using Core.Application.Behaviors;
using Core.Application.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cach.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Cach_AddInfrastructure(this IServiceCollection services
            , IConfiguration configuration
            )
        {
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.ConfigureCachingServices(configuration);
            return services;
        }

        private static IServiceCollection ConfigureCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? new CacheSettings();
            var redisConnection = configuration.GetConnectionString("Redis");

            
            if (cacheSettings.UseRedis && !string.IsNullOrEmpty(redisConnection))
            {
                try
                {
                    ConfigureRedisCache(services, cacheSettings, redisConnection);
                }
                catch (Exception ex)
                {
                    ConfigureMemoryCache(services);
                 }
            }
            else
            {
                ConfigureMemoryCache(services);
            }

            return services;
        }

        private static void ConfigureRedisCache(IServiceCollection services, CacheSettings cacheSettings, string redisConnection)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = cacheSettings.RedisInstanceName;
            });
            // ثبت IConnectionMultiplexer به‌صورت Singleton برای استفاده در RedisCacheService
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));
            services.AddScoped<ICachePublicService, RedisCacheService>();

         }

        private static void ConfigureMemoryCache(IServiceCollection services )
        {
            services.AddMemoryCache();
            services.AddScoped<ICachePublicService, MemoryCacheService>();

        }
    }
}
