using Core.Application.Abstractions.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Events
{
    public class MediatorEventBus : IEventBus
    {
        private readonly IMediator _mediator;

        public MediatorEventBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : INotification
        {
            return _mediator.Publish(@event);
        }

        public async Task PublishAsync<TEvent>(params TEvent[] events) where TEvent : INotification
        {
            foreach (var @event in events)
            {
                await _mediator.Publish(@event);
            }
        }
    }
}