using Cach.Infrastructure.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cach.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection Cach_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");
            var redis = config.GetConnectionString("Redis");

            services.AddHealthChecks()
                    //.AddDbContextCheck<>("AuthDb")
                    .AddRedis(redis, name: "Redis")
                    .AddCheck<CacheHealthCheck>("Cache");


            return services;
        }
    }
}
