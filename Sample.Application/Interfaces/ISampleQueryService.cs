using Core.Shared.Results;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Interfaces
{
    public interface ISampleQueryService
    {
        Task<Result<IReadOnlyList<SampleEntity>>> GetBySpecAsync(string property1, int page = 1, int pageSize = 10);
        Task<Result<IReadOnlyList<SampleEntity>>> GetCachedSamplesAsync(string property1);
    }
}
