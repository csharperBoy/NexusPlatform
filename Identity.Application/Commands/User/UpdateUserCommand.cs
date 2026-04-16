using Core.Domain.ValueObjects;
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
    public record UpdateUserCommand(
     Guid Id,
     string UserName,
     string? NickName,
     string? Password,
     string? Email,
     string? phoneNumber,
     Guid? personId = null,
     List<string>? roles = null
 ) : IRequest<Result<bool>>;
}
