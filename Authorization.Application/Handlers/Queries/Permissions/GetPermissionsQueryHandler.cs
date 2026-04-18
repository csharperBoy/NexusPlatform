using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Permissions;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Permissions
{
    
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<IList<PermissionDto>>>
    {
        private readonly IPermissionInternalService _permissionService;
        public GetPermissionsQueryHandler(IPermissionInternalService permissionService)
            => _permissionService = permissionService;

        public async Task<Result<IList<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken ct)
        {
            IReadOnlyList<PermissionDto> permissions = await _permissionService.GetPermissions(request);
            return Result<IList<PermissionDto>>.Ok(permissions.ToList());
        }
    }
}
