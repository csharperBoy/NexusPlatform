using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.DependencyInjection
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
                _logger.LogInformation("Starting identity module initialization...");

                // اجرای seed داده‌ها
            
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