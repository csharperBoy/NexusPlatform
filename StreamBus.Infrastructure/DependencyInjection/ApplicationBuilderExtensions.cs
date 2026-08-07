using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
namespace StreamBus.Infrastructure.DependencyInjection
{
   
    public static class ApplicationBuilderExtensions
    {
        // 📌 متد اصلی برای استفاده در Startup
        public static async Task<IApplicationBuilder> StreamBus_UseInfrastructure(this IApplicationBuilder app)
        {
            await app.RunSmartMigrations();
            return app;
        }

        // 📌 اجرای Migrationها به صورت هوشمند
        private static async Task<IApplicationBuilder> RunSmartMigrations(this IApplicationBuilder app)
        {
            

            return app;
        }
    }
}
