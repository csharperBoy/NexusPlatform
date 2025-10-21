using Auth.Infrastructure.DependencyInjection;
using Auth.Presentation.DependencyInjection;
using Auth.Application.DependencyInjection;
using Core.Infrastructure.DependencyInjection;
using People.Infrastructure.DependencyInjection;
using Core.Infrastructure.HealthChecks;
using Auth.Infrastructure.Data;
using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Serilog;

try
{
    Log.Information("🚀 Starting AkSteel Welfare Platform application...");

    var builder = WebApplication.CreateBuilder(args);

    // اضافه کردن Configuration
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    var configuration = builder.Configuration;

    // Core Infrastructure (حالا شامل Serilog هست)
    builder.Services.AddCoreInfrastructure(configuration);

    // ماژول‌های برنامه
    builder.Services.AddAuthApplication(configuration);
    builder.Services.AddAuthInfrastructure(configuration);
    builder.Services.AddAuthPresentation(configuration);
    builder.Services.AddPeopleInfrastructure(configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();

    //var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    //builder.Services.AddCors(options =>
    //{
    //    options.AddPolicy("AppCorsPolicy", policy =>
    //    {
    //        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
    //              .AllowAnyHeader()
    //              .AllowAnyMethod()
    //              .AllowCredentials();
    //    });
    //});

    var app = builder.Build();

    // استفاده از Correlation ID Middleware
    app.UseMiddleware<CorrelationIdMiddleware>();

    // اجرای Migrationهای هوشمند
    await RunSmartMigrations(app);

    // سلامت‌سنجی در Startup
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var healthCheck = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("🔍 Running AkSteel Welfare Platform health checks...");

            var systemStatus = await healthCheck.GetSystemStatusAsync();
            var cacheStatus = await healthCheck.GetCacheStatusAsync();

            logger.LogInformation("🏥 System Health: {IsHealthy}", systemStatus.IsHealthy ? "✅ Healthy" : "❌ Unhealthy");
            logger.LogInformation("💾 Cache: {Message} ({ResponseTimeMs}ms)", cacheStatus.Message, cacheStatus.ResponseTimeMs);

            if (!systemStatus.IsHealthy)
            {
                logger.LogWarning("⚠️ Warning: System has health issues - check configuration");
            }
            else
            {
                logger.LogInformation("🎉 All systems are ready!");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Health check failed during startup");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AppCorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("🎉 AkSteel Welfare Platform started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

async Task RunSmartMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🚀 Starting database migrations...");

        var dbContextTypes = new[]
        {
            typeof(AuthDbContext),
            //typeof(UserManagementDbContext)
        };

        foreach (var dbContextType in dbContextTypes)
        {
            try
            {
                logger.LogInformation("🔧 Migrating {DbContext}...", dbContextType.Name);

                var method = typeof(IMigrationManager).GetMethod(nameof(IMigrationManager.MigrateAsync));
                var genericMethod = method.MakeGenericMethod(dbContextType);
                await (Task)genericMethod.Invoke(migrationManager, new object[] { default(CancellationToken) });

                logger.LogInformation("✅ {DbContext} migrated successfully", dbContextType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to migrate {DbContext}", dbContextType.Name);

                if (app.Environment.IsDevelopment())
                {
                    throw;
                }
            }
        }

        logger.LogInformation("🎉 All migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "💥 Migration process failed");
        if (app.Environment.IsProduction())
        {
            logger.LogWarning("Continuing in production despite migration failures");
        }
        else
        {
            throw;
        }
    }
}