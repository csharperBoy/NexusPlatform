using Core.Shared.Results;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries;
using Identity.Application.Queries.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.User
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IList<UserDto>>>
    {
        private readonly IUserInternalService _userService;
        public GetUsersQueryHandler(IUserInternalService userService)
            => _userService = userService;

        public async Task<Result<IList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
        {
           var users = await _userService.getUsers(request);
            return Result<IList<UserDto>>.Ok(users.ToList());
        }
    }
}
