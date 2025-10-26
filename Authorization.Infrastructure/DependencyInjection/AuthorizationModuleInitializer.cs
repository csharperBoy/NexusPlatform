using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.DependencyInjection
{
    public class AuthorizationModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthorizationModuleInitializer> _logger;

        public AuthorizationModuleInitializer(IServiceProvider serviceProvider, ILogger<AuthorizationModuleInitializer> logger)
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
                //var context = services.GetRequiredService<AuthDbContext>();
                //await context.Database.MigrateAsync(cancellationToken);

                // اجرای seed داده‌ها

                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                await AuthorizationSeedData.SeedRolesAsync(roleManager);

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