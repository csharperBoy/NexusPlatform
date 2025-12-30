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
    public class EffectivePermissionsSpec : BaseSpecification<Permission>
    {
        public EffectivePermissionsSpec(Guid? personId, Guid? positionId, List<Guid>? roleIds, string resourceKey, PermissionAction action)
            : base(p => p.Resource.Key == resourceKey &&
                        p.Action == action &&
                        (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow) &&
                        (p.EffectiveFrom == null || p.EffectiveFrom <= DateTime.UtcNow) &&
                        (
                            (p.AssigneeType == AssigneeType.Person && p.AssigneeId == personId) ||
                            (positionId.HasValue && p.AssigneeType == AssigneeType.Position && p.AssigneeId == positionId.Value) ||
                            (p.AssigneeType == AssigneeType.Role && roleIds.Contains(p.AssigneeId))
                        ))
        {
            AddInclude(p => p.Resource);
        }
    }
}
