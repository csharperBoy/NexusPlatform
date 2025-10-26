using Core.Application.Abstractions.Caching;
using Core.Application.Models;
using Core.Infrastructure.Caching;
using Core.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.DependencyInjection
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
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

            return services;
        }

        private static void ConfigureMemoryCache(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
            Console.WriteLine("✅ In-Memory Cache configured");
        }
    }
}