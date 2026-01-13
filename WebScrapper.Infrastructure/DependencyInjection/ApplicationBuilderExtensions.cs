using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Infrastructure.Data;
using Serilog;
namespace WebScrapper.Infrastructure.DependencyInjection
{
   

    public static class ApplicationBuilderExtensions
    {
        // 📌 متد اصلی برای استفاده در Startup
        public static async Task<IApplicationBuilder> WebScrapper_UseInfrastructure(this IApplicationBuilder app)
        {
            await app.RunSmartMigrations();
            return app;
        }

        // 📌 اجرای Migrationها به صورت هوشمند
        private static async Task<IApplicationBuilder> RunSmartMigrations(this IApplicationBuilder app)
        {
           /* using var scope = app.ApplicationServices.CreateScope();

            var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var dbContextType = typeof(SampleDbContext);
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"Migrations.{dbContextType.Name}");

            try
            {
                logger.LogInformation("🚀 Starting database migrations...");

                try
                {
                    logger.LogInformation("🔧 Migrating {DbContext}...", dbContextType.Name);

                    // 📌 اجرای متد Generic MigrateAsync برای SampleDbContext
                    var method = typeof(IMigrationManager).GetMethod(nameof(IMigrationManager.MigrateAsync));
                    var genericMethod = method!.MakeGenericMethod(dbContextType);
                    await (Task)genericMethod.Invoke(migrationManager, new object[] { CancellationToken.None })!;

                    logger.LogInformation("✅ {DbContext} migrated successfully", dbContextType.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Failed to migrate {DbContext}", dbContextType.Name);

                    if (env.IsDevelopment())
                    {
                        // در محیط Development خطا دوباره throw می‌شود
                        throw;
                    }
                }

                logger.LogInformation("🎉 All migrations completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "💥 Migration process failed");

                if (env.IsProduction())
                {
                    // در Production برنامه ادامه پیدا می‌کند حتی اگر Migration شکست بخورد
                    logger.LogWarning("Continuing in production despite migration failures");
                }
                else
                {
                    throw;
                }
            }
            */
            return app;
        }
    }
}
