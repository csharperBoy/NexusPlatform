using Authorization.Domain.Enums;
using Core.Domain.Enums;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.Permissions
{
    /*
     📌 AssignPermissionCommand
     --------------------------
     Command برای ایجاد یک Permission جدید.
    */
    public record AssignPermissionCommand(

        Guid ResourceId,
        AssigneeType AssigneeType,
        Guid AssigneeId,
        PermissionAction Action,
         ScopeType scope = ScopeType.None,
            Guid? specificScopeId = null,
            PermissionType type = PermissionType.allow,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        string? Description = null
    ) : IRequest<Result<Guid>>;
}
