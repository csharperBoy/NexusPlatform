// Core.Infrastructure/Events/OutboxProcessor.cs
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
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

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
                return;
            }

            _logger.LogDebug("Processing {Count} outbox messages for {DbContext}",
                messages.Count, typeof(TDbContext).Name);

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

                var eventType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == message.Type && typeof(IDomainEvent).IsAssignableFrom(t));

                if (eventType == null)
                {
                    throw new InvalidOperationException($"Event type {message.Type} not found");
                }

                var domainEvent = System.Text.Json.JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize event {message.Type}");
                }

                await eventBus.PublishAsync(domainEvent);
                await outboxService.MarkAsCompletedAsync(message.Id);

                _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                await outboxService.MarkAsFailedAsync(message.Id, ex.Message);
            }
        }
    }
}