using Navigation.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Infrastructure.DependencyInjection
{
    

    public static class HealthCheckExtensions
    {
        public static IServiceCollection Navigation_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            // 📌 گرفتن Connection String (در صورت نیاز برای Health Checkهای سفارشی)
            var conn = config.GetConnectionString("DefaultConnection");

            // 📌 اضافه کردن Health Check برای دیتابیس BaseDbContext
            services.AddHealthChecks()
                    .AddDbContextCheck<NavigationDbContext>("BaseDatabase");

            return services;
        }
    }
}
