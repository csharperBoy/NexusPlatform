using Core.Infrastructure.DependencyInjection;
using HR.IrisaSync.Extention;
using HR.IrisaSync.Extention.Contexts;
using Microsoft.EntityFrameworkCore;


//using Notification.Presentation.Hubs;
using Serilog;

try
{
    //#region test

    //using (var context = new IrisaOracleDbContext())
    //{
    //    var a = context.PdsIdeaInformationViws.Where(a => a.NumPrsnEmply == 868 || a.NumPrsnEmply == 310).ToList();
    //}
    //#endregion

    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Log.Information("🚀 Starting AkSteel Management application...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration
         .SetBasePath(builder.Environment.ContentRootPath)
         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
         .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables();

    var configuration = builder.Configuration;
    builder.Services.AddEnableModulesServiceCollectionExtensions(configuration);
    builder.Services.AddEnableModulesServiceCollectionExtensions_InApp(configuration);

    var app = builder.Build();
    await app.UseEnableModulesApplicationBuilderExtensions(configuration);
    await app.UseSwaggerApplicationBuilderExtensions(configuration);

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    //app.MapHub<NotificationHub>("/hubs/notifications");

    Log.Information("🎉  AkSteel Management application started successfully");

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var server = app.Services.GetRequiredService<
            Microsoft.AspNetCore.Hosting.Server.IServer>();
        var addresses = server.Features
            .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

        Console.WriteLine("🚀 Application started. Listening on:");
        foreach (var address in addresses!.Addresses)
        {
            Console.WriteLine($" {address}");
        }
    });
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
