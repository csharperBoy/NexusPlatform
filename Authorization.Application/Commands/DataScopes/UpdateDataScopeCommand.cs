using Authorization.Domain.Enums;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.DataScopes
{
    /*
       📌 UpdateDataScopeCommand
       -------------------------
       Command برای بروزرسانی DataScope موجود.
      */
    public record UpdateDataScopeCommand(
        Guid DataScopeId,
        ScopeType Scope,
        Guid? SpecificUnitId = null
    ) : IRequest<Result<bool>>;
}
