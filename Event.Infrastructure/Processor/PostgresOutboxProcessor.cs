// Core.Infrastructure/Events/PostgresOutboxProcessor.cs
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Event.Infrastructure.Processor
{
    public class PostgresOutboxProcessor<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly string _connectionString;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PostgresOutboxProcessor<TDbContext>> _logger;

        public PostgresOutboxProcessor(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<PostgresOutboxProcessor<TDbContext>> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PostgreSQL Outbox Processor started");

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(stoppingToken);

                await CreateTriggerFunction(connection);

                connection.Notification += OnPostgresNotification;
                await using (var cmd = new NpgsqlCommand("LISTEN outbox_notification;", connection))
                {
                    await cmd.ExecuteNonQueryAsync(stoppingToken);
                }

                _logger.LogInformation("PostgreSQL LISTEN/NOTIFY started for Outbox");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await connection.WaitAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostgreSQL Outbox Processor failed, falling back to polling");
                await StartFallbackProcessor(stoppingToken);
            }
        }

        private async void OnPostgresNotification(object? sender, NpgsqlNotificationEventArgs e)
        {
            _logger.LogDebug("Received PostgreSQL notification: {Payload}", e.Payload);
            await ProcessNewMessages();
        }

        private async Task ProcessNewMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService<TDbContext>>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var messages = await outboxService.GetPendingMessagesAsync(100);

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

                _logger.LogDebug("Processed outbox message {MessageId} via PostgreSQL processor", message.Id);
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

        private async Task CreateTriggerFunction(NpgsqlConnection connection)
        {
            try
            {
                var functionSql = @"
                    CREATE OR REPLACE FUNCTION notify_outbox_change()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        PERFORM pg_notify('outbox_notification', NEW.""Id""::text);
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DROP TRIGGER IF EXISTS outbox_notify_trigger ON ""OutboxMessages"";
                    CREATE TRIGGER outbox_notify_trigger
                    AFTER INSERT ON ""OutboxMessages""
                    FOR EACH ROW EXECUTE FUNCTION notify_outbox_change();
                ";

                await using var cmd = new NpgsqlCommand(functionSql, connection);
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation("PostgreSQL trigger function created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create PostgreSQL trigger function (might already exist)");
            }
        }

        private async Task StartFallbackProcessor(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting fallback polling processor for PostgreSQL");
            var fallbackProcessor = ActivatorUtilities.CreateInstance<OutboxProcessor<TDbContext>>(_serviceProvider);
            await fallbackProcessor.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PostgreSQL Outbox Processor stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}