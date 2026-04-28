using Core.Shared.DTOs;
using Core.Shared.Results;
using Identity.Application.Interfaces;
using Identity.Application.Queries.Role;
using Identity.Application.Queries.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.Role
{
    public class GetRolesSelectionListQueryHandler : IRequestHandler<GetRolesSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IRoleInternalService _roleService;
        public GetRolesSelectionListQueryHandler(  IRoleInternalService roleService)
            => _roleService = roleService;

        public async Task<Result<IList<SelectionListDto>>> Handle(GetRolesSelectionListQuery request, CancellationToken ct)
        {

            var roles = await _roleService.getRoles(request.Name,request.description);
            var result = roles.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Name}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
