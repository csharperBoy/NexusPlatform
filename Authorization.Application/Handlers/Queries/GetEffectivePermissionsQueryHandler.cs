using Authorization.Application.Interfaces;
using Authorization.Application.Queries;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries
{
    
    public class GetEffectivePermissionsQueryHandler
      : IRequestHandler<GetEffectivePermissionsQuery, Result<List<string>>>
    {
        private readonly IPermissionService _service;

        public GetEffectivePermissionsQueryHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<List<string>>> Handle(GetEffectivePermissionsQuery req, CancellationToken ct)
        {
            var list = await _service.GetEffectivePermissionsAsync(req.UserId, ct);
            return Result<List<string>>.Success(list);
        }
    }
}
