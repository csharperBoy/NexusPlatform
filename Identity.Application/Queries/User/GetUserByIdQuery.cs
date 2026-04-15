using Core.Shared.DTOs.Authorization;
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

    public record GetUserByIdQuery(Guid Id)
        : IRequest<Result<UserDto>>;
}
