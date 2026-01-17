using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Infrastructure.Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Trader.Server.Collector.Infrastructure.DependencyInjection
{

    public static class HealthCheckExtensions
    {
        public static IServiceCollection Sample_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            // 📌 گرفتن Connection String (در صورت نیاز برای Health Checkهای سفارشی)
            var conn = config.GetConnectionString("DefaultConnection");

            // 📌 اضافه کردن Health Check برای دیتابیس SampleDbContext
            services.AddHealthChecks()
                    .AddDbContextCheck<CollectorDbContext>("CollectorDatabase");

            return services;
        }
    }
}
