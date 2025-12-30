using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Enums;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class PermissionConflictSpec : BaseSpecification<Permission>
    {
        public PermissionConflictSpec(AssigneeType assigneeType, Guid assigneeId, Guid resourceId, PermissionAction action)
            : base(p => p.AssigneeType == assigneeType &&
                       p.AssigneeId == assigneeId &&
                       p.ResourceId == resourceId &&
                       p.Action == action &&
                       (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow) && // !IsExpired
                       (p.EffectiveFrom == null || p.EffectiveFrom <= DateTime.UtcNow)) // IsEffective
        {
            AddInclude(p => p.Resource);
        }
    }
}
