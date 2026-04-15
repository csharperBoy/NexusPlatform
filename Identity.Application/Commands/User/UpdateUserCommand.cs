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
     string? FirstName,
     string? LastName,
     string? Password,
     string? Email,
      string? phoneNumber
 ) : IRequest<Result<bool>>;
}
