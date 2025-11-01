// Core.Infrastructure/Events/SqlDependencyOutboxProcessor.cs
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Event.Infrastructure.Processor
{
    public class SqlDependencyOutboxProcessor<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly string _connectionString;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SqlDependencyOutboxProcessor<TDbContext>> _logger;
        private SqlDependency? _dependency;

        public SqlDependencyOutboxProcessor(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<SqlDependencyOutboxProcessor<TDbContext>> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeSqlDependency();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private async Task InitializeSqlDependency()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await EnableServiceBroker(connection);

                using var command = new SqlCommand(
                    "SELECT [Id] FROM [Audit].[OutboxMessages] WHERE [Status] = 0",
                    connection);

                _dependency = new SqlDependency(command);
                _dependency.OnChange += OnOutboxTableChanged;

                using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                _logger.LogInformation("SQL Dependency initialized for Outbox table");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize SQL Dependency");
                await StartFallbackProcessor();
            }
        }

        private async void OnOutboxTableChanged(object? sender, SqlNotificationEventArgs e)
        {
            _logger.LogInformation("Outbox table change detected. Type: {Type}", e.Type);

            try
            {
                await ProcessNewMessages();

                if (_dependency != null)
                {
                    _dependency.OnChange -= OnOutboxTableChanged;
                }
                await InitializeSqlDependency();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling outbox table change");
            }
        }

        private async Task ProcessNewMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService<TDbContext>>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var messages = await outboxService.GetPendingMessagesAsync(50);

            foreach (var message in messages)
            {
                await ProcessSingleMessageAsync(message, outboxService, eventBus);
            }
        }

        private async Task ProcessSingleMessageAsync(OutboxMessage message, IOutboxService<TDbContext> outboxService, IEventBus eventBus)
        {
            try
            {
                await outboxService.MarkAsProcessingAsync(message.Id);

                var eventType = ResolveEventType(message);
                if (eventType == null)
                    throw new InvalidOperationException($"Event type {message.TypeName} not found");

                var domainEvent = (IDomainEvent?)System.Text.Json.JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent == null)
                    throw new InvalidOperationException($"Failed to deserialize event {message.TypeName}");

                await eventBus.PublishAsync(domainEvent);
                await outboxService.MarkAsCompletedAsync(message.Id);

                _logger.LogDebug("Processed outbox message {MessageId}", message.Id);
            }
            catch (DbUpdateConcurrencyException cex)
            {
                _logger.LogWarning(cex, "Concurrency conflict while processing message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                await outboxService.MarkAsFailedAsync(message.Id, ex.Message);
            }
        }

        private static Type? ResolveEventType(OutboxMessage message)
        {
            var aqn = message.AssemblyQualifiedName;
            if (!string.IsNullOrWhiteSpace(aqn))
            {
                return Type.GetType(aqn, throwOnError: false);
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == message.TypeName && typeof(IDomainEvent).IsAssignableFrom(t));
        }

        private async Task EnableServiceBroker(SqlConnection connection)
        {
            var enableBrokerCommand = new SqlCommand(
                "IF (SELECT is_broker_enabled FROM sys.databases WHERE name = DB_NAME()) = 0 " +
                "ALTER DATABASE CURRENT SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE",
                connection);

            await enableBrokerCommand.ExecuteNonQueryAsync();
        }

        private async Task StartFallbackProcessor()
        {
            _logger.LogInformation("Starting fallback polling processor");
            var fallbackProcessor = ActivatorUtilities.CreateInstance<OutboxProcessor<TDbContext>>(_serviceProvider);
            await fallbackProcessor.StartAsync(CancellationToken.None);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_dependency != null)
            {
                _dependency.OnChange -= OnOutboxTableChanged;
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
