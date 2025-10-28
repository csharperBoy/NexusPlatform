using Audit.Infrastructure.Data;
using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Audit.Infrastructure.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> Audit_UseInfrastructure(this IApplicationBuilder app)
        {
            await app.RunSmartMigrations();
            return app;
        }

        private static async Task<IApplicationBuilder> RunSmartMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            // چون کلاس static است، از ILoggerFactory برای ساخت لاگر با دسته‌بندی سفارشی استفاده کن
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Migrations");

            try
            {
                logger.LogInformation("🚀 Starting database migrations...");

                var dbContextType = typeof(AuditDbContext);

                try
                {
                    logger.LogInformation("🔧 Migrating {DbContext}...", dbContextType.Name);

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
                    logger.LogWarning("Continuing in production despite migration failures");
                }
                else
                {
                    throw;
                }
            }

            return app;
        }
    

    }
}
