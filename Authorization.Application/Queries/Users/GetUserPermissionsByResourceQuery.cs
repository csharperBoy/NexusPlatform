using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Users
{
    public record GetUserPermissionsByResourceQuery(Guid UserId, string ResourceKey)
     : IRequest<Result<EffectivePermissionDto>>;

}
