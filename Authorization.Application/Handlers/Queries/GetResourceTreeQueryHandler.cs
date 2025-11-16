using Authorization.Application.Queries;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries
{
    public class GetResourceTreeQueryHandler
       : IRequestHandler<GetResourceTreeQuery, Result<List<object>>>
    {
        private readonly IResourceService _service;

        public GetResourceTreeQueryHandler(IResourceService service)
        {
            _service = service;
        }

        public async Task<Result<List<object>>> Handle(GetResourceTreeQuery request, CancellationToken ct)
        {
            var tree = await _service.GetResourceTreeAsync(ct);
            return Result<List<object>>.Success(tree);
        }
    }
}
