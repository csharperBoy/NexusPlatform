using Auth.Infrastructure.DependencyInjection;
using Auth.Presentation.DependencyInjection;
using Auth.Application.DependencyInjection;
using Core.Infrastructure.DependencyInjection;
using People.Infrastructure.DependencyInjection;
using Core.Infrastructure.HealthChecks;
using Auth.Infrastructure.Data;
using Core.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن Configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// configuration
var configuration = builder.Configuration;

builder.Services.AddCoreInfrastructure(configuration);

builder.Services.AddAuthApplication(configuration);
builder.Services.AddAuthInfrastructure(configuration);
builder.Services.AddAuthPresentation(configuration);

builder.Services.AddPeopleInfrastructure(configuration);




builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
var app = builder.Build();

// اجرای Migrationهای هوشمند
await RunSmartMigrations(app);

// سلامت‌سنجی در Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var healthCheck = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

        Console.WriteLine("🔍 Running AkSteel Welfare Platform health checks...");

        var systemStatus = await healthCheck.GetSystemStatusAsync();
        //var dbStatus = await healthCheck.GetDatabaseStatusAsync();
        var cacheStatus = await healthCheck.GetCacheStatusAsync();

        Console.WriteLine($"🏥 System Health: {(systemStatus.IsHealthy ? "✅ Healthy" : "❌ Unhealthy")}");
        //Console.WriteLine($"🗄️ Database: {dbStatus.Message} ({dbStatus.ResponseTimeMs}ms)");
        Console.WriteLine($"💾 Cache: {cacheStatus.Message} ({cacheStatus.ResponseTimeMs}ms)");

        if (!systemStatus.IsHealthy)
        {
            Console.WriteLine("⚠️  Warning: System has health issues - check configuration");
        }
        else
        {
            Console.WriteLine("🎉 All systems are ready!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Health check failed: {ex.Message}");
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

app.Run();



async Task RunSmartMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🚀 Starting database migrations...");

        // لیست DbContextها برای migration
        var dbContextTypes = new[]
        {
            typeof(AuthDbContext),
            //typeof(UserManagementDbContext)
            // اضافه کردن بقیه
        };

        foreach (var dbContextType in dbContextTypes)
        {
            try
            {
                logger.LogInformation("🔧 Migrating {DbContext}...", dbContextType.Name);

                // اجرای migration با reflection
                var method = typeof(IMigrationManager).GetMethod(nameof(IMigrationManager.MigrateAsync));
                var genericMethod = method.MakeGenericMethod(dbContextType);
                await (Task)genericMethod.Invoke(migrationManager, new object[] { default(CancellationToken) });

                logger.LogInformation("✅ {DbContext} migrated successfully", dbContextType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to migrate {DbContext}", dbContextType.Name);

                // در Production ادامه بده، در Development متوقف شو
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
            // در Production برنامه رو متوقف نکن
            logger.LogWarning("Continuing in production despite migration failures");
        }
        else
        {
            throw;
        }
    }
}