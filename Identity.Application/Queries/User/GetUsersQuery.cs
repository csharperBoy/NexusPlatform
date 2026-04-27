using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Queries.User
{
    public record GetUsersQuery(string? UserName = null, List<Guid>? rolesId = null, string? NickName = null , string? phoneNumber = null) : IRequest<Result<IList<UserDto>>>;

}
