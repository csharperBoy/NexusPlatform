using Core.Shared.Results;
using Identity.Application.Interfaces;
using Identity.Application.Queries.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries
{
    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<IList<string>>>
    {
        private readonly IRoleInternalService _roleService;
        public GetUserRolesQueryHandler(IRoleInternalService roleService)
            => _roleService = roleService;

        public async Task<Result<IList<string>>> Handle(GetUserRolesQuery request, CancellationToken ct)
        {
            var roles = await _roleService.GetUserRolesAsync(request.UserId);
            return Result<IList<string>>.Ok(roles);
        }
    }
}
