// Core.Infrastructure/Events/HybridOutboxProcessor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Events
{
    public class HybridOutboxProcessor<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HybridOutboxProcessor<TDbContext>> _logger;
        private readonly IConfiguration _configuration;
        private BackgroundService? _activeProcessor;

        public HybridOutboxProcessor(
            IServiceProvider serviceProvider,
            ILogger<HybridOutboxProcessor<TDbContext>> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var databaseProvider = _configuration.GetValue<string>("Database:Provider")?.ToLower();

            try
            {
                _activeProcessor = databaseProvider switch
                {
                    "sqlserver" => CreateSqlServerProcessor(),
                    "postgresql" => CreatePostgresProcessor(),
                    _ => CreatePollingProcessor()
                };

                if (_activeProcessor != null)
                {
                    _logger.LogInformation("Starting {ProcessorType} for Outbox processing",
                        _activeProcessor.GetType().Name);

                    await _activeProcessor.StartAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start event-driven processor, falling back to polling");
                _activeProcessor = CreatePollingProcessor();
                await _activeProcessor.StartAsync(stoppingToken);
            }
        }

        private BackgroundService CreateSqlServerProcessor()
        {
            return ActivatorUtilities.CreateInstance<SqlDependencyOutboxProcessor<TDbContext>>(
                _serviceProvider, _configuration);
        }

        private BackgroundService CreatePostgresProcessor()
        {
            _logger.LogWarning("PostgreSQL real-time processor not implemented, using polling");
            return CreatePollingProcessor();
        }

        private BackgroundService CreatePollingProcessor()
        {
            return ActivatorUtilities.CreateInstance<OutboxProcessor<TDbContext>>(_serviceProvider);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_activeProcessor != null)
            {
                await _activeProcessor.StopAsync(cancellationToken);
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
