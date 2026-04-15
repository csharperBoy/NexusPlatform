using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands.User
{
    public record CreateUserCommand(
        string UserName, 
        string Password, 
        string Email,
        string? NickName,
        string? phoneNumber,
        Guid? personId = null
) : IRequest<Result<Guid>>;
}
