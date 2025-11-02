// Core.Infrastructure/Events/TriggerBasedOutboxProcessor.cs
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Event.Infrastructure.Processor
{
    public class TriggerBasedOutboxProcessor<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TriggerBasedOutboxProcessor<TDbContext>> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

        public TriggerBasedOutboxProcessor(
            IServiceProvider serviceProvider,
            ILogger<TriggerBasedOutboxProcessor<TDbContext>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trigger-based Outbox Processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForNewMessagesAsync(stoppingToken);
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error in trigger-based outbox processor");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task CheckForNewMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService<TDbContext>>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var messages = await outboxService.GetPendingMessagesAsync(100);

            if (messages.Any())
            {
                _logger.LogDebug("Found {Count} new messages to process", messages.Count());

                foreach (var message in messages)
                {
                    await ProcessSingleMessageAsync(message, outboxService, eventBus);
                }
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
                await outboxService.MarkAsFailedAsync(message.Id, ex);
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
    }
}