using Core.Shared.Results;
using Sample.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Interfaces
{
    public interface ISampleService
    {

        Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request);
        Task<Result<SampleApiResponse>> sampleEventHandlerMethodAsync(object property1);
    }
}
