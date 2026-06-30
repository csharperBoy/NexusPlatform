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
    public class PermissionsByPersonSpec : BaseSpecification<Permission>
    {
        public PermissionsByPersonSpec(Guid personId)
            : base(p => p.FkPermissionAssigneeId == personId)
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.FkResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
