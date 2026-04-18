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
     📌 RevokePermissionCommand
     --------------------------
     Command برای حذف یک Permission.
    */
    public record DeletePermissionCommand(
        Guid Id
    ) : IRequest<Result<bool>>;
}
