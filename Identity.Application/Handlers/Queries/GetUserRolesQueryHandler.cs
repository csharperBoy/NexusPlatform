using Core.Shared.Results;
using Identity.Application.Interfaces;
using Identity.Application.Queries;
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
        private readonly IAuthorizationService _authorizationService;
        public GetUserRolesQueryHandler(IAuthorizationService authorizationService)
            => _authorizationService = authorizationService;

        public async Task<Result<IList<string>>> Handle(GetUserRolesQuery request, CancellationToken ct)
        {
            var roles = await _authorizationService.GetUserRolesAsync(request.UserId);
            return Result<IList<string>>.Ok(roles);
        }
    }
}
