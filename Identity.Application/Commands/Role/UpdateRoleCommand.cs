using Core.Domain.ValueObjects;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands.Role
{
    public record UpdateRoleCommand(
     Guid Id,
     string Name,
      string Description,
        int OrderNum
 ) : IRequest<Result<bool>>;
}
