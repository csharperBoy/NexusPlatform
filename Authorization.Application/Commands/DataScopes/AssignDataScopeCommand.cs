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
     📌 AssignDataScopeCommand
     -------------------------
     Command برای اختصاص DataScope به Assignee (Role/Position/Person).
    */
    public record AssignDataScopeCommand(
    Guid ResourceId,
    AssigneeType AssigneeType,
    Guid AssigneeId,
    ScopeType Scope,
    Guid? SpecificUnitId = null,
    string CustomFilter = "",
    int Depth = 1,
    DateTime? EffectiveFrom = null,
    DateTime? ExpiresAt = null,
    string Description = ""
) : IRequest<Result<Guid>>;
}
