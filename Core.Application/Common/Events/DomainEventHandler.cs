using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Common.Events
{
    public abstract class DomainEventHandler<TEvent> : IEventHandler<TEvent>
      where TEvent : IDomainEvent
    {
        protected readonly ILogger<DomainEventHandler<TEvent>> _logger;

        public DomainEventHandler(ILogger<DomainEventHandler<TEvent>> logger)
        {
            _logger = logger;
        }

        public virtual async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling domain event: {EventType}", typeof(TEvent).Name);
                await HandleEventAsync(@event, cancellationToken);
                _logger.LogInformation("Domain event handled successfully: {EventType}", typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event: {EventType}", typeof(TEvent).Name);
                throw;
            }
        }

        protected abstract Task HandleEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
