using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands
{
    public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest<Result>;
    public record GetUserRolesQuery(Guid UserId) : IRequest<Result<IList<string>>>;
}
