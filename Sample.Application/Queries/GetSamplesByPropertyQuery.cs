using Core.Shared.Results;
using MediatR;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Queries
{
    public record GetSamplesByPropertyQuery(string property1, int Page = 1, int PageSize = 10)
        : IRequest<Result<IReadOnlyList<SampleEntity>>>;
}
