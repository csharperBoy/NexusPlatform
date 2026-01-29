using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.DependencyInjection
{
    public class ModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleInitializer> _logger;
        private readonly IConfiguration _configuration;
        public ModuleInitializer(IServiceProvider serviceProvider, ILogger<ModuleInitializer> logger, IConfiguration configuration)
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
                _logger.LogInformation("Starting Audit module initialization...");

                // اجرای seed داده‌ها
                var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                var resourceService = scope.ServiceProvider.GetRequiredService<IResourcePublicService>();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionPublicService>();
                var roleService = scope.ServiceProvider.GetRequiredService<IRolePublicService>();

                // روش 1: استفاده از متد یکپارچه
                await AuditSeedData.SeedAsync(
                    resourceService, permissionService, roleService,
                    _logger);
                _logger.LogInformation("Successfull Audit module initialization.");



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the identity module");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}