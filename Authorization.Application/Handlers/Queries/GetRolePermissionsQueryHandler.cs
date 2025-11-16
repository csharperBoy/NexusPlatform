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
    public class GetRolePermissionsQueryHandler
        : IRequestHandler<GetRolePermissionsQuery, Result<List<Guid>>>
    {
        private readonly IPermissionService _service;

        public GetRolePermissionsQueryHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<List<Guid>>> Handle(GetRolePermissionsQuery req, CancellationToken ct)
        {
            var ids = await _service.GetRolePermissionIdsAsync(req.RoleId, ct);
            return Result<List<Guid>>.Success(ids);
        }
    }
}
