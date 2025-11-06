using Core.Domain.Events;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Sample.Application.Interfaces;

namespace Sample.Application.EventHandlers
{
    public class SampleEventHandler : INotificationHandler<SampleEvent>
    {
        private readonly ISampleService _sampleService;
        private readonly ILogger<SampleEventHandler> _logger;

        public SampleEventHandler(
            ISampleService sampleService,
            ILogger<SampleEventHandler> logger)
        {
            _sampleService = sampleService;
            _logger = logger;
        }

        public async Task Handle(SampleEvent sample, CancellationToken cancellationToken)
        {
            await _sampleService.SampleApiMethodAsync(sample.property1);

        }
    }
}

