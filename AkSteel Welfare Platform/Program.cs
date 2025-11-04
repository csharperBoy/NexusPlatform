using Audit.Application.DependencyInjection;
using Audit.Infrastructure.Data;
using Audit.Infrastructure.DependencyInjection;
using Audit.Presentation.DependencyInjection;
using Authentication.Application.DependencyInjection;
using Authentication.Infrastructure.Data;
using Authentication.Infrastructure.DependencyInjection;
using Authentication.Presentation.DependencyInjection;
using Authorization.Application.DependencyInjection;
using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.DependencyInjection;
using Authorization.Presentation.DependencyInjection;
using Cach.Application.DependencyInjection;
using Cach.Infrastructure.DependencyInjection;
using Cach.Presentation.DependencyInjection;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Database;
using Core.Infrastructure.DependencyInjection;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Middlewares;
using Core.Presentation.DependencyInjection;
using Event.Infrastructure.DependencyInjection;
using Notification.Application.DependencyInjection;
using Notification.Infrastructure.DependencyInjection;
using Notification.Presentation.DependencyInjection;
using Notification.Presentation.Hubs;
using Serilog;
using User.Application.DependencyInjection;
using User.Infrastructure.Data;
using User.Infrastructure.DependencyInjection;
using User.Presentation.DependencyInjection;

try
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Log.Information("🚀 Starting AkSteel Welfare Platform application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    var configuration = builder.Configuration;
    builder.Services.AddEnableModulesServiceCollectionExtensions(configuration);
    //builder.Services.User_AddInfrastructure(configuration);
  
    var app = builder.Build();

    await app.UseEnableModulesApplicationBuilderExtensions(configuration);
    await app.UseSwaggerApplicationBuilderExtensions(configuration);

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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
