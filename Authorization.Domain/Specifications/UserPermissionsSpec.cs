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
        public UserPermissionsSpec(Guid userId, Guid? partyId, List<Guid>? postIds, List<Guid> roleIds)
            : base(p =>
                (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow) &&
                (p.EffectiveFrom == null || p.EffectiveFrom <= DateTime.UtcNow) &&
                (
                    ( p.FkPermissionAssigneeId == userId) ||
                    (partyId != null ? ( p.FkPermissionAssigneeId == partyId) : false) ||
                    (postIds != null ? (postIds.Count() > 0  && postIds.Any(q => q == p.FkPermissionAssigneeId)) : false) ||
                   (roleIds != null ? (roleIds.Count() > 0  && roleIds.Any(q => q == p.FkPermissionAssigneeId)) : false)
                ))
        {
            AddInclude(p => p.Resource);
            AddInclude(p => p.PermissionAssignee);
            AddInclude(p => p.Rules);
            
        }
    }
}
