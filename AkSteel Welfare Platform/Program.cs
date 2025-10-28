using Authentication.Infrastructure.DependencyInjection;
using Authentication.Presentation.DependencyInjection;
using Authentication.Application.DependencyInjection;
using Core.Infrastructure.DependencyInjection;
using Core.Presentation.DependencyInjection;
using Core.Infrastructure.HealthChecks;
using Authentication.Infrastructure.Data;
using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Serilog;
using Authorization.Application.DependencyInjection;
using Notification.Application.DependencyInjection;
using Notification.Presentation.DependencyInjection;
using Notification.Presentation.Hubs;
using Core.Infrastructure.Middlewares;
using User.Infrastructure.Data;
using Authorization.Infrastructure.Data;
using Cach.Infrastructure.DependencyInjection;
using Cach.Application.DependencyInjection;
using Cach.Presentation.DependencyInjection;
using Audit.Infrastructure.Data;
using Audit.Infrastructure.DependencyInjection;
using Audit.Application.DependencyInjection;
using Audit.Presentation.DependencyInjection;

using Authorization.Infrastructure.DependencyInjection;
using Authorization.Application.DependencyInjection;
using Authorization.Presentation.DependencyInjection;
using Notification.Infrastructure.DependencyInjection;
using User.Application.DependencyInjection;
using User.Infrastructure.DependencyInjection;
using User.Presentation.DependencyInjection;

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
    builder.Services.Core_AddInfrastructure(configuration);
    builder.Services.Core_AddHealthChecks(configuration);
    builder.Services.Core_AddPresentation(configuration);
    // ماژول‌های برنامه
    builder.Services.Cach_AddApplication(configuration);
    builder.Services.Cach_AddInfrastructure(configuration);
    builder.Services.Cach_AddPresentation(configuration);
    builder.Services.Cach_AddHealthChecks(configuration);

    builder.Services.Audit_AddApplication(configuration);
    builder.Services.Audit_AddInfrastructure(configuration);
    builder.Services.Audit_AddPresentation(configuration);
    builder.Services.Audit_AddHealthChecks(configuration);


    builder.Services.Auth_AddApplication(configuration);
    builder.Services.Auth_AddInfrastructure(configuration);
    builder.Services.Auth_AddPresentation(configuration);
    builder.Services.Auth_AddHealthChecks(configuration);
    
    builder.Services.Authorization_AddApplication(configuration);
    builder.Services.Authorization_AddInfrastructure(configuration);
    builder.Services.Authorization_AddPresentation(configuration);
    builder.Services.Authorization_AddHealthChecks(configuration);

    builder.Services.Notification_AddApplication(configuration);
    builder.Services.Notification_AddInfrastructure(configuration);
    builder.Services.Notification_AddPresentation(configuration);
    builder.Services.Notification_AddHealthChecks(configuration);

    builder.Services.User_AddApplication(configuration);
    builder.Services.User_AddInfrastructure(configuration);
    builder.Services.User_AddPresentation(configuration);
    builder.Services.User_AddHealthChecks(configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();

    var app = builder.Build();
    app.Core_UseInfrastructure();
    // استفاده از Correlation ID Middleware
    // اجرای Migrationهای هوشمند
    //await RunSmartMigrations(app);

    // سلامت‌سنجی در Startup
   /* using (var scope = app.Services.CreateScope())
    {
        try
        {
            //var healthCheck = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("🔍 Running AkSteel Welfare Platform health checks...");

            var systemStatus = await healthCheck.GetSystemStatusAsync();

            logger.LogInformation("🏥 System Health: {IsHealthy}", systemStatus.IsHealthy ? "✅ Healthy" : "❌ Unhealthy");

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
*/

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    //app.UseNotificationPresentation();
    app.MapHub<NotificationHub>("/hubs/notifications");

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
