using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.DependencyInjection
{
    public class IdentityModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IdentityModuleInitializer> _logger;
        private readonly IConfiguration _configuration;
        public IdentityModuleInitializer(IServiceProvider serviceProvider, ILogger<IdentityModuleInitializer> logger, IConfiguration configuration)
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
                _logger.LogInformation("Starting identity module initialization...");

                // اجرای seed داده‌ها
            
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                await IdentitySeedData.SeedRolesAsync(roleManager);
                await IdentitySeedData.SeedAdminUserAsync(userManager, _configuration);
                _logger.LogInformation("Identity module initialization completed successfully.");
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