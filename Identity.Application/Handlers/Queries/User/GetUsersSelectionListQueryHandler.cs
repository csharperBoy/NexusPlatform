using Core.Shared.DTOs;
using Core.Shared.Results;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.User
{
    public class GetUsersSelectionListQueryHandler : IRequestHandler<GetUsersSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IUserInternalService _userService;
        public GetUsersSelectionListQueryHandler(IUserInternalService userService)
            => _userService = userService;

        public async Task<Result<IList<SelectionListDto>>> Handle(GetUsersSelectionListQuery request, CancellationToken ct)
        {
            var users = await _userService.GetUsers(request.UserName, request.rolesId, request.NickName, request.phoneNumber);
            var result = users.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.UserName} ({x.NickName})"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
