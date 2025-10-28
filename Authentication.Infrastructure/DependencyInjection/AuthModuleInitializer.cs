using Authentication.Domain.Entities;
using Authentication.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.Entity;

namespace Authentication.Infrastructure.DependencyInjection
{
    public class AuthModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthModuleInitializer> _logger;
        private readonly IConfiguration _configuration;
        public AuthModuleInitializer(IServiceProvider serviceProvider, ILogger<AuthModuleInitializer> logger, IConfiguration configuration)
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
                _logger.LogInformation("Starting Auth module initialization...");

                // اجرای seed داده‌ها
            
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await AuthenticationSeedData.SeedAdminUserAsync(userManager, _configuration);
                _logger.LogInformation("Auth module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the Auth module");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}