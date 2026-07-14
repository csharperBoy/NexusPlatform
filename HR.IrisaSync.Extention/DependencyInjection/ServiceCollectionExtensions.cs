
using Core.Application.Abstractions;
using Core.Infrastructure.Database;
using Core.Infrastructure.Repositories;
using HR.Infrastructure.Data;
using HR.IrisaSync.Extention.Contexts;
using HR.IrisaSync.Extention.Controller;
using HR.IrisaSync.Extention.Data;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using HR.IrisaSync.Extention.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HR.IrisaSync.Extention.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
      
        public static IServiceCollection IrisaSync_AddDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(IrisaExtentionDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            services.AddDbContext<IrisaExtentionDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__IrisaSyncMigrationsHistory", "hr");
                });
            });

            services.AddDbContext<IrisaOracleDbContext>((serviceProvider, options) =>
            {
                //options.UseSqlServer(conn, b =>
                //{
                //    // تعیین Assembly محل Migrationها
                //    b.MigrationsAssembly(migrationsAssembly);

                //    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                //    b.MigrationsHistoryTable("__HRHistory", "hr");
                //});
                options.UseOracle("User Id=TPOUT_PDS;Password=irisatpout;Data Source=//192.168.7.5:1521/prod;");
            });
            services.AddHealthChecks()
                    .AddDbContextCheck<IrisaExtentionDbContext>("IrisaExtentionDatabase");

            services.AddHealthChecks()
                    .AddDbContextCheck<IrisaOracleDbContext>("IrisaOracleDatabase");

            //services.AddScoped<SyncService>();
            //services.AddScoped<ISyncService>(sp => sp.GetRequiredService<SyncService>());
            services.AddScoped<ISyncService, SyncService>();
            services.AddScoped<IMapService, MapService>();
            
            services.AddScoped<IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string>, EfRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string>>();
            services.AddScoped<ISpecificationRepository<PdsIdeaInformationViw, string>, EfSpecificationRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string>>();

            services.AddScoped<IIrisaSyncUnitOfWork<IrisaExtentionDbContext>, IrisaSyncUnitOfWork>();

            services.AddControllers()
             .AddApplicationPart(typeof(IrisaSyncController).Assembly)
             .AddControllersAsServices();

            services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(IrisaSyncController).Assembly));

            return services;
        }

        public static async Task<IApplicationBuilder> IrisaSync_UseDependency(this IApplicationBuilder app)
        {
            await app.RunSmartMigrations();

            return app;
        }
        private static async Task<IApplicationBuilder> RunSmartMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var dbContextType = typeof(IrisaExtentionDbContext);
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