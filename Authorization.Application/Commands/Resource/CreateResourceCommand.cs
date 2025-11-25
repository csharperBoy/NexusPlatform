using Authorization.Domain.Enums;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands
{ /*
     📌 CreateResourceCommand
     ------------------------
     Command برای ایجاد Resource جدید.
    */
    public record CreateResourceCommand(
        string Key,
        string Name,
        ResourceType Type,
        ResourceCategory Category,
        Guid? ParentId = null
    ) : IRequest<Result<Guid>>;
}
