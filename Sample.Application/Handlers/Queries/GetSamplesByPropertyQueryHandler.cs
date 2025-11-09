using Core.Shared.Results;
using MediatR;
using Sample.Application.Interfaces;
using Sample.Application.Queries;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Handlers.Queries
{
    public class GetSamplesByPropertyQueryHandler
       : IRequestHandler<GetSamplesByPropertyQuery, Result<IReadOnlyList<SampleEntity>>>
    {
        private readonly ISampleQueryService _sampleService;

        public GetSamplesByPropertyQueryHandler(ISampleQueryService sampleService)
        {
            _sampleService = sampleService;
        }

        public async Task<Result<IReadOnlyList<SampleEntity>>> Handle(GetSamplesByPropertyQuery request, CancellationToken ct)
        {
            return await _sampleService.GetBySpecAsync(request.property1, request.Page, request.PageSize);
        }
    }
}
