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
     📌 TogglePermissionCommand
     --------------------------
     Command برای تغییر allow/deny یک Permission.
    */
    public record UpdatePermissionCommand(
        Guid Id,
        Guid? ResourceId = null,
        AssigneeType? AssigneeType = null,
        Guid? AssigneeId = null,
        PermissionAction? Action = null,
        PermissionEffect? effect = null,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        bool? IsActive = null,
        string? Description = null,

        List<ScopeType>? scopes = null
    ) : IRequest<Result<bool>>;
}
