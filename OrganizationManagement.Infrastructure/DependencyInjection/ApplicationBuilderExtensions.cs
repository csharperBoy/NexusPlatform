using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrganizationManagement.Infrastructure.Data;
using Serilog;
namespace OrganizationManagement.Infrastructure.DependencyInjection
{
    /*
     📌 ApplicationBuilderExtensions
     --------------------------------
     این کلاس یک Extension برای IApplicationBuilder است که وظیفه‌اش اجرای Migrationهای دیتابیس
     در زمان راه‌اندازی برنامه (Startup) می‌باشد.

     ✅ نکات کلیدی:
     - متد اصلی: Sample_UseInfrastructure
       → این متد در زمان راه‌اندازی برنامه فراخوانی می‌شود و متد RunSmartMigrations را اجرا می‌کند.
     - متد RunSmartMigrations:
       1. یک Scope جدید از DI Container ایجاد می‌کند.
       2. سرویس IMigrationManager را دریافت می‌کند (مدیریت Migrationها).
       3. محیط اجرا (Development/Production) را بررسی می‌کند.
       4. تلاش می‌کند Migrationها را اجرا کند:
          - اگر موفق بود → لاگ موفقیت ثبت می‌شود.
          - اگر خطا رخ داد:
            - در Development → Exception دوباره throw می‌شود (برای رفع سریع مشکل).
            - در Production → فقط لاگ هشدار ثبت می‌شود و برنامه ادامه پیدا می‌کند.
       5. در پایان لاگ موفقیت یا شکست ثبت می‌شود.

     🛠 جریان کار:
     1. برنامه در Startup یا Program.cs متد Sample_UseInfrastructure را فراخوانی می‌کند.
     2. این متد RunSmartMigrations را اجرا می‌کند.
     3. MigrationManager بررسی می‌کند که آیا Migrationهای جدید وجود دارند یا نه.
     4. اگر وجود داشتند، آن‌ها را اعمال می‌کند.
     5. لاگ‌ها وضعیت عملیات را ثبت می‌کنند.

     📌 نتیجه:
     این کلاس تضمین می‌کند که دیتابیس ماژول Sample همیشه به‌روز باشد
     و Migrationها به صورت هوشمند (Smart) در زمان راه‌اندازی اعمال شوند،
     بدون اینکه اجرای برنامه در Production به خاطر خطا متوقف شود.
    */

    public static class ApplicationBuilderExtensions
    {
        // 📌 متد اصلی برای استفاده در Startup
        public static async Task<IApplicationBuilder> OrganizationManagement_UseInfrastructure(this IApplicationBuilder app)
        {
            await app.RunSmartMigrations();
            return app;
        }

        // 📌 اجرای Migrationها به صورت هوشمند
        private static async Task<IApplicationBuilder> RunSmartMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var dbContextType = typeof(OrganizationManagementDbContext);
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

            return app;
        }
    }
}
