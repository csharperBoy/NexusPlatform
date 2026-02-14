using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Specifications;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class UserPermissionsSpec : BaseSpecification<Permission>
    {
        public UserPermissionsSpec(Guid userId, Guid? personId, List<Guid>? positionsId, List<Guid> roleIds)
            : base(p =>
                (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow) &&
                (p.EffectiveFrom == null || p.EffectiveFrom <= DateTime.UtcNow) &&
                (
                    (p.AssigneeType == AssigneeType.User && p.AssigneeId == userId) ||
                    (p.AssigneeType == AssigneeType.Person && p.AssigneeId == personId) ||
                    (positionsId.Count()>0 && p.AssigneeType == AssigneeType.Position && positionsId.Any(q=>q ==  p.AssigneeId )) ||
                    (p.AssigneeType == AssigneeType.Role && roleIds.Contains(p.AssigneeId))
                ))
        {
            AddInclude(p => p.Resource);
        }
    }
}
