using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.Navigation.PublicService;
using EPM.Application.Abstractions;
using EPM.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPM.Infrastructure.DependencyInjection;

public class ModuleInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModuleInitializer> _logger;
    private readonly IConfiguration _configuration;

    public ModuleInitializer(
        IServiceProvider serviceProvider,
        ILogger<ModuleInitializer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            _logger.LogInformation("Starting Epm module initialization...");

            // ─── Authorization + Navigation Seed ───
            var resourceService = services.GetRequiredService<IResourcePublicService>();
            var permissionService = services.GetRequiredService<IPermissionPublicService>();
            var roleService = services.GetRequiredService<IRolePublicService>();
            var menuService = services.GetRequiredService<IMenuPublicService>();

            await EpmSeedData.SeedEpmForAuthorizationAsync(
                resourceService, permissionService, roleService, _logger);

            await EpmSeedData.SeedEpmsForNavigationAsync(menuService, _logger);

            // ─── Trading Schedules Seed ───
            //var uow = services.GetRequiredService<IEpmUnitOfWork<EpmDbContext>>();
            
            _logger.LogInformation("Epm module initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the Epm module");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}