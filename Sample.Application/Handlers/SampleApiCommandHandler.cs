using Core.Shared.Results;
using MediatR;
using Sample.Application.Commands;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;

namespace Sample.Application.Handlers
{
    public class SampleApiCommandHandler : IRequestHandler<SampleApiCommand, Result<SampleApiResponse>>
    {
    
        private readonly ISampleService _sampleService;

        public SampleApiCommandHandler(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        public async Task<Result<SampleApiResponse>> Handle(SampleApiCommand request, CancellationToken cancellationToken)
        {
            return await _sampleService.SampleApiMethodAsync(new SampleApiRequest(request.property1, request.property2));

        }
    }

}
