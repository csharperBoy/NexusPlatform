using Auth.Infrastructure.Data;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.DependencyInjection
{
    public class AuthModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthModuleInitializer> _logger;

        public AuthModuleInitializer(IServiceProvider serviceProvider, ILogger<AuthModuleInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                _logger.LogInformation("Starting Auth module initialization...");

                // اجرای میگریشن
                var context = services.GetRequiredService<AuthDbContext>();
                await context.Database.MigrateAsync(cancellationToken);

                // اجرای seed داده‌ها
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await SeedData.SeedRolesAndAdminAsync(roleManager, userManager);

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