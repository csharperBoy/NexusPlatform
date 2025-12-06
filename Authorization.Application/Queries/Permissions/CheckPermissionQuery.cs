using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Permissions
{
    
    public record CheckPermissionQuery(Guid UserId, string ResourceKey, string Action)
     : IRequest<Result<bool>>;
}
