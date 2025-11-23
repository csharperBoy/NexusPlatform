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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ResourceRegistrarHostedService> _logger;

        public ResourceRegistrarHostedService(IServiceProvider serviceProvider, ILogger<ResourceRegistrarHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var providers = scope.ServiceProvider.GetServices<IPermissionDefinitionProvider>().ToList();
            var permissionService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

            var bag = new ConcurrentBag<PermissionDescriptor>();
            var ctx = new PermissionDefinitionContext(bag);

            foreach (var p in providers)
            {
                try
                {
                    p.Define(ctx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in permission provider {Provider}", p.GetType().FullName);
                }
            }

            var descriptors = bag.ToArray();
            if (descriptors.Any())
            {
                _logger.LogInformation("Registering {Count} permissions from {ProvidersCount} providers", descriptors.Length, providers.Count);
                await permissionService.RegisterPermissionsAsync(descriptors, cancellationToken);
            }
            else
            {
                _logger.LogInformation("No permission descriptors found from providers");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
