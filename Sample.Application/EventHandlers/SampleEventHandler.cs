using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Events;

namespace Sample.Application.EventHandlers
{
    public class SampleEventHandler : INotificationHandler<SampleActionEvent>
    {
        private readonly ISampleService _sampleService;
        private readonly ILogger<SampleEventHandler> _logger;

        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public SampleEventHandler(
            ISampleService sampleService,
            ILogger<SampleEventHandler> logger
            , IReadOnlyPolicyRegistry<string> policies)
        {
            _sampleService = sampleService;
            _logger = logger;
            _policies = policies;
        }

        public async Task Handle(SampleActionEvent sample, CancellationToken cancellationToken)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _sampleService.SampleApiMethodAsync(new SampleApiRequest(sample.ActionProperty1, ""));
            }, cancellationToken);

        }
    }
}

