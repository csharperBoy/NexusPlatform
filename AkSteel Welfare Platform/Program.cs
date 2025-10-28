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
using Authorization.Presentation.DependencyInjection;
using Notification.Infrastructure.DependencyInjection;
using User.Application.DependencyInjection;
using User.Infrastructure.DependencyInjection;
using User.Presentation.DependencyInjection;

try
{
    Log.Information("🚀 Starting AkSteel Welfare Platform application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    var configuration = builder.Configuration;

    builder.Services.AddEnableModulesServiceCollectionExtensions(configuration);
  
    var app = builder.Build();

    await app.UseEnableModulesApplicationBuilderExtensions(configuration);


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
