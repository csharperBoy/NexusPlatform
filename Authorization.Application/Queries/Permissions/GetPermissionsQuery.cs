using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Permissions
{

    public record GetPermissionsQuery(AssigneeType? AssigneeType = null,
         Guid? AssigneeId = null,
     Guid? ResourceId = null,
    string? description = null) : IRequest<Result<IList<PermissionDto>>>;

}
