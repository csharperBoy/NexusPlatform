using Authorization.Domain.Enums;
using Core.Domain.Enums;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
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
    public record CreatePermissionCommand(

        Guid ResourceId,
        Guid AssigneeId,
        AssigneeType AssigneeType = AssigneeType.User,
        PermissionAction Action = PermissionAction.Full,
        PermissionEffect effect = PermissionEffect.allow,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        bool IsActive = true,
        string? Description = null,
        
        List<ScopeType>? scopes = null // لیست محدوده های مجاز یا غیر مجاز

    ) : IRequest<Result<Guid>>;
}
