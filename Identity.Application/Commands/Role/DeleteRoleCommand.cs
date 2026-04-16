using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands.Role
{
    public record DeleteRoleCommand(
        Guid Id
    ) : IRequest<Result<bool>>;
}
