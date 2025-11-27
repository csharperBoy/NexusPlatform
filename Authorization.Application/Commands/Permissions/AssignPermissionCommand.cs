using Authorization.Domain.Enums;
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
        bool IsAllow = true,
        DateTime? EffectiveFrom = null,
        DateTime? ExpiresAt = null,
        string? Description = null,
        int Order = 0,
        int Priority = 1,
        string Condition = ""
    ) : IRequest<Result<Guid>>;
}
