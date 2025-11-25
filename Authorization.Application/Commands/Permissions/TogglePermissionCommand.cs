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
    public record TogglePermissionCommand(
        Guid PermissionId,
        bool IsAllow
    ) : IRequest<Result<bool>>;
}
