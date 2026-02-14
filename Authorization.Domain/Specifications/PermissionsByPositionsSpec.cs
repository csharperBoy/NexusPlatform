using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class PermissionsByPositionsSpec : BaseSpecification<Permission>
    {
        public PermissionsByPositionsSpec(List<Guid> positionIds)
            : base(p => p.AssigneeType == AssigneeType.Position && // تغییر از User به Person
                      positionIds.Any(r => r == p.AssigneeId))
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.ResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
