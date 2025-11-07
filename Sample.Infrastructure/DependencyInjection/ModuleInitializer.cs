using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Domain.Entities;
using Sample.Infrastructure.Data;

namespace Sample.Infrastructure.DependencyInjection
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
                _logger.LogInformation("Starting sample module initialization...");

                // اجرای seed داده‌ها

                var repo = scope.ServiceProvider.GetRequiredService<IRepository<SampleDbContext, SampleEntity, Guid>>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<SampleDbContext>>();
                await SampleSeedData.SeedEntityAsync(repo,uow, _configuration,_logger);

                _logger.LogInformation("Sample module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the Sample module");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}