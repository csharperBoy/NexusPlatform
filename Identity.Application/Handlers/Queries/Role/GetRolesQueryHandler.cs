using Core.Shared.Results;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries;
using Identity.Application.Queries.Role;
using Identity.Application.Queries.Role;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.Role
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<IList<RoleDto>>>
    {
        private readonly IRoleInternalService _roleService;
        public GetRolesQueryHandler(IRoleInternalService roleService)
            => _roleService = roleService;

        public async Task<Result<IList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken ct)
        {
           var roles = await _roleService.getRoles(request);
            return Result<IList<RoleDto>>.Ok(roles.ToList());
        }
    }
}
