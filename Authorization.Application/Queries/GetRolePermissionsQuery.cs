using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries
{
    public record GetRolePermissionsQuery(Guid RoleId)
       : IRequest<Result<List<Guid>>>;

}
