using Authorization.Application.Interfaces;
using Authorization.Application.Security;
using Authorization.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.HostedServices
{
    public class ResourceRegistrarHostedService : IHostedService
    {
        private readonly ILogger<ResourceRegistrarHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;
        private bool _isInitialized = false;

        public ResourceRegistrarHostedService(
            ILogger<ResourceRegistrarHostedService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resource Registrar Hosted Service is starting...");

            try
            {
                // ثبت اولیه منابع
                await InitializeResourcesAsync();
                _isInitialized = true;

                // تنظیم تایمر برای sync دوره‌ای (هر 24 ساعت)
                _timer = new Timer(
                    callback: async _ => await SyncResourcesAsync(),
                    state: null,
                    dueTime: TimeSpan.FromHours(24),
                    period: TimeSpan.FromHours(24)
                );

                _logger.LogInformation("Resource Registrar Hosted Service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Resource Registrar Hosted Service");
                throw;
            }
        }

        private async Task InitializeResourcesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();

            _logger.LogInformation("Initial resource registration started...");

            await resourceService.RegisterAllModulesResourcesAsync();

            // همگام‌سازی برای اطمینان از consistency
            await resourceService.SyncResourcesWithDefinitionsAsync();

            _logger.LogInformation("Initial resource registration completed");
        }

        private async Task SyncResourcesAsync()
        {
            if (!_isInitialized) return;

            try
            {
                _logger.LogInformation("Periodic resource synchronization started...");

                using var scope = _serviceProvider.CreateScope();
                var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();

                await resourceService.SyncResourcesWithDefinitionsAsync();

                _logger.LogInformation("Periodic resource synchronization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic resource synchronization");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resource Registrar Hosted Service is stopping...");

            _timer?.Change(Timeout.Infinite, 0);

            // انجام sync نهایی قبل از shutdown
            if (_isInitialized)
            {
                await SyncResourcesAsync();
            }

            _logger.LogInformation("Resource Registrar Hosted Service stopped");
        }
    }
}
