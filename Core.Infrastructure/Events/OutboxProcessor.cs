﻿// Core.Infrastructure/Events/OutboxProcessor.cs
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Events
{
    public class OutboxProcessor<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor<TDbContext>> _logger;
        private TimeSpan _interval = TimeSpan.FromSeconds(30);

        public OutboxProcessor(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessor<TDbContext>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor for {DbContext} started", typeof(TDbContext).Name);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);

                    // Adaptive interval based on workload (simple heuristic)
                    _interval = TimeSpan.FromSeconds(_interval.TotalSeconds is > 5 and < 60
                        ? _interval.TotalSeconds
                        : 30);

                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error processing outbox messages for {DbContext}", typeof(TDbContext).Name);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("Outbox Processor for {DbContext} stopped", typeof(TDbContext).Name);
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService<TDbContext>>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var messages = (await outboxService.GetPendingMessagesAsync(50)).ToList();

            if (!messages.Any())
            {
                _interval = TimeSpan.FromSeconds(Math.Min(_interval.TotalSeconds * 2, 60));
                return;
            }

            _interval = TimeSpan.FromSeconds(10);

            foreach (var message in messages)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

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

                _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
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
            // Prefer AQN; fallback to scanning
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