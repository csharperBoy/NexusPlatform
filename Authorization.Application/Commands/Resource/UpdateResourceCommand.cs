using Authorization.Domain.Enums;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.Resource
{
    /*
      📌 UpdateResourceCommand
      ------------------------
      Command برای به‌روزرسانی Resource.
     */
    public record UpdateResourceCommand(
     Guid Id,
     string Name,
     string Description,
     ResourceType Type,
     ResourceCategory Category,
     int DisplayOrder,
     string Icon,
     string Route,
     Guid? ParentId = null // ✅ اضافه شده
 ) : IRequest<Result<bool>>;
}
